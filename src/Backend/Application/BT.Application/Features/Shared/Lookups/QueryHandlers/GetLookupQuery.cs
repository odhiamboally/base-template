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
using BT.Domain.Features.Shared.Lookups.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.Shared.Lookups.Dtos;
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
    public string Discriminator => GetLookupRequest.LookupType;
    public string? CacheUserId => null;
    public bool IsVersioned => false;
}

