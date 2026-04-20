using BT.Domain.Contracts.Interfaces.Common;
using BT.Persistence.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Common;

public abstract class BaseUnitOfWork<TContext>(
    TContext context,
    IPublisher publisher,
    ILogger logger
) where TContext : DbContext
{
    protected readonly TContext _context = context;
    private readonly IPublisher _publisher = publisher;
    private readonly ILogger _logger = logger;

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

    public async Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation, int maxRetries = 3, int baseDelayMs = 50)
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
        => await _context.SaveChangesAsync(ct).ConfigureAwait(false);

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
            await _publisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
    }
}
