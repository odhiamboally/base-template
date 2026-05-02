using BT.Application.Contracts.Interfaces.Common;
using BT.SharedKernel.Extensions;
using BT.Application.Utilities;
using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Shared.FailedMessages.Enums;
using BT.Domain.Features.Shared.Lookups.Enums;
using BT.Domain.Features.Shared.Outbox.Enums;
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Dtos.Lookups;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Features.Shared.Lookups.QueryHandlers;


public sealed record GetLookupQuery(GetLookupRequest GetLookupRequest, string UserId) 
    : IRequest<AppResponse<IReadOnlyList<LookupResponse>>>, ICachableRequest
{
    public string CacheGroup => "lookups";
    public string Discriminator => "all";
    public string? CacheUserId => UserId;
    public bool IsVersioned => true;
}

internal sealed class GetLookupQueryHandler(ISharedUnitOfWork _sharedUnitOfWork, ILogger<GetLookupQueryHandler> _logger) 
    : IRequestHandler<GetLookupQuery, AppResponse<IReadOnlyList<LookupResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<LookupResponse>>> Handle(GetLookupQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var req = query.GetLookupRequest;

            // Convert string description to enum
            if (!Enum.TryParse<LookupType>(req.LookupType, true, out var enumValue))
            {
                try
                {
                    enumValue = req.LookupType.ToEnum<LookupType>();
                }
                catch (ArgumentException ex)
                {
                    LookupLogDefinitions.LogInvalidLookupType(_logger, req.LookupType, ex);

                    return  new AppResponse<IReadOnlyList<LookupResponse>>
                    {
                        Successful = false,
                        Message = $"Invalid lookup type: {req.LookupType}"
                    };
                        
                }
            }

            var lookups = await _sharedUnitOfWork.LookupRepository
                .GetLookupsByTypeAsync(enumValue, cancellationToken)
                .ConfigureAwait(false);

            var response = lookups.Select(l => new LookupResponse(l.Id, l.Code, l.Description)).ToList();

            return new AppResponse<IReadOnlyList<LookupResponse>>
            {
                Successful = true,
                Data = response
            };
            
        }
        catch (ArgumentOutOfRangeException ex)
        {
            LookupLogDefinitions.LogInvalidLookupType(_logger, query.GetLookupRequest.LookupType, ex);

            return new AppResponse<IReadOnlyList<LookupResponse>>
            {
                Successful = false,
                Message = $"Unsupported lookup type: {query.GetLookupRequest.LookupType}"
            };
                
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(_logger, nameof(GetLookupQueryHandler), ex);
                
            throw;
            throw;
        }

    }
}
    
