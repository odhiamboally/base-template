using BT.Domain.Contracts.Interfaces.Common;
using BT.Domain.Contracts.Interfaces.Repositories;
using BT.Persistence.Common;
using BT.Persistence.Logging;
using BT.Persistence.Shared.DataContext;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BT.Persistence.Shared;

public sealed class SharedUnitOfWork(
    SharedDbContext context,
    ILookupRepository lookupRepository,
    IEmailTemplateRepository emailTemplateRepository,
    IFailedMessageRepository failedMessageRepository,
    IPublishEndpoint publishEndpoint,
    IPublisher publisher,
    ILogger<SharedUnitOfWork> logger
) : BaseUnitOfWork<SharedDbContext>(context, publisher, logger), ISharedUnitOfWork
{
    private readonly SharedDbContext _sharedContext = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<SharedUnitOfWork> _logger = logger;

    public ILookupRepository LookupRepository { get; } = lookupRepository;
    public IEmailTemplateRepository EmailTemplateRepository { get; } = emailTemplateRepository;
    public IFailedMessageRepository FailedMessageRepository { get; } = failedMessageRepository;

    public async Task<int> CompleteWithEventsAsync(List<IIntegrationEvent>? appEvents = null, CancellationToken ct = default)
    {
        await using var transaction = await _sharedContext.Database.BeginTransactionAsync(ct);

        try
        {
            var result = await _sharedContext.SaveChangesAsync(ct).ConfigureAwait(false);
            var domainEvents = _sharedContext.GetCollectedDomainEvents() ?? Array.Empty<IDomainEvent>();

            foreach (var domainEvent in domainEvents)
                await _publishEndpoint.Publish(domainEvent, ct).ConfigureAwait(false);

            if (appEvents != null)
                foreach (var appEvent in appEvents)
                    await _publishEndpoint.Publish(appEvent, ct);

            await transaction.CommitAsync(ct).ConfigureAwait(false);
            _sharedContext.ClearCollectedDomainEvents();

            PersistenceLogDefinitions.LogEventsPublished(_logger, domainEvents.Count);
            return result;
        }
        catch (Exception ex)
        {
            PersistenceLogDefinitions.LogCompleteWithEventsRollback(_logger, ex);
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            _sharedContext.ClearCollectedDomainEvents();
            throw;
        }
    }
}
