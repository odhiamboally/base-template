using System.Collections.ObjectModel;
using BT.Application.Contracts.Interfaces.Common;
using BT.Application.Features.IAM.Menus.Mappings;
using BT.Application.Utilities;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Features.IAM.Menus.Entities;
using BT.SharedKernel.Dtos.Common;
using BT.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BT.Application.Features.IAM.Menus.Queries;


public sealed record SearchMenusQuery(MenuSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<MenuResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "menus";
    public string Discriminator => CacheKeys.Discriminator(SearchRequest);
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

