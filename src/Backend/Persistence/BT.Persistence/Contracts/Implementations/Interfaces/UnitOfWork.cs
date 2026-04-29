using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Banking.Contracts.Repositories;
using BT.Domain.HR.Contracts.Repositories;
using BT.Domain.IAM.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Persistence.Logging;
using BT.Persistence.DataContext;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Interfaces;

public class UnitOfWork(
    IUserRepository userRepository,
    ICustomerRepository customerRepository,
    IEmployeeRepository employeeRepository,
    ILookupRepository lookupRepository,
    IEmailTemplateRepository emailTemplateRepository,
    IFailedMessageRepository failedMessageRepository,
    ISessionRepository sessionRepository,
    ITempTotpSecretRepository tempTotpSecretRepository,
    IAppUserTotpSecretRepository appUserTotpSecretRepository,
    IAppUserProfileRepository appUserProfileRepository,
    ITokenRepository tokenRepository,

    DBContext context,

    IPublishEndpoint publishEndpoint,
    IPublisher publisher,
    ILogger<UnitOfWork> logger


) : IIamUnitOfWork, IHrUnitOfWork, IBankingUnitOfWork, ISharedUnitOfWork
{
    public IUserRepository UserRepository { get; private set; } = userRepository;
    public ICustomerRepository CustomerRepository { get; private set; } = customerRepository;
    public IEmployeeRepository EmployeeRepository { get; private set; } = employeeRepository;
    public ILookupRepository LookupRepository { get; private set; } = lookupRepository;
    public IEmailTemplateRepository EmailTemplateRepository { get; private set; } = emailTemplateRepository;
    public IFailedMessageRepository FailedMessageRepository { get; private set; } = failedMessageRepository;
    public ISessionRepository SessionRepository { get; private set; } = sessionRepository;
    public ITempTotpSecretRepository TempTotpSecretRepository { get; private set; } = tempTotpSecretRepository;
    public IAppUserTotpSecretRepository AppUserTotpSecretRepository { get; private set; } = appUserTotpSecretRepository;
    public IAppUserProfileRepository AppUserProfileRepository { get; private set; } = appUserProfileRepository;
    public ITokenRepository TokenRepository { get; private set; } = tokenRepository;

    private readonly DBContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IPublisher _publisher = publisher;
    private readonly ILogger<UnitOfWork> _logger = logger;

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await operation().ConfigureAwait(false);

                await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                PersistenceLogDefinitions.LogTransactionConcurrencyRollback(_logger, ex);

                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                _context.ChangeTracker.Clear();

                throw;
            }
            catch (Exception ex)
            {
                PersistenceLogDefinitions.LogTransactionErrorRollback(_logger, ex);

                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }

        }).ConfigureAwait(false);
    }

    public async Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries, int baseDelayMs)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var result = await operation().ConfigureAwait(false);

                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);

                    return result;
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    _context.ChangeTracker.Clear();

                    await Task.Delay(TimeSpan.FromMilliseconds(baseDelayMs * attempt)).ConfigureAwait(false);

                    continue;
                }
                catch
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            }

            throw new InvalidOperationException("Max retry attempts exceeded due to concurrency conflicts.");

        }).ConfigureAwait(false);
    }


    public async Task<int> CompleteAsync(CancellationToken ct = default)
    {
        var result = await _context.SaveChangesAsync(ct).ConfigureAwait(false);
        return result!;
    }

    public async Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default)
    {
        // Use a transaction to ensure atomicity
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            // Save changes and collect domain events
            var result = await _context.SaveChangesAsync(ct).ConfigureAwait(false);
            var domainEvents = _context.GetCollectedDomainEvents() ?? Array.Empty<IDomainEvent>();

            // Publish domain events (MassTransit outbox will store them)
            foreach (var domainEvent in domainEvents)
            {
                await _publishEndpoint.Publish(domainEvent, ct).ConfigureAwait(false);
            }

            // Publish application events if provided
            if (appEvents != null)
            {
                foreach (var appEvent in appEvents)
                {
                    await _publishEndpoint.Publish(appEvent, ct).ConfigureAwait(false);
                }
            }

            // Commit transaction - if this succeeds, everything is saved
            await transaction.CommitAsync(ct).ConfigureAwait(false);

            // Clear events after successful commit
            _context.ClearCollectedDomainEvents();

            PersistenceLogDefinitions.LogEventsPublished(_logger, domainEvents.Count);

            return result;
        }
        catch (Exception ex)
        {
            PersistenceLogDefinitions.LogCompleteWithEventsRollback(_logger, ex);
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            _context.ClearCollectedDomainEvents();
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        //GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }

    

    public IReadOnlyList<IDomainEvent> GetPendingDomainEvents() => _context.GetCollectedDomainEvents() ?? Array.Empty<IDomainEvent>();
       
    public void ClearDomainEvents() => _context.ClearCollectedDomainEvents();

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities.SelectMany(x => x.DomainEvents).ToList();

        domainEntities.ForEach(x => x.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

}
