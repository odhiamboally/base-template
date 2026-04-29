using BT.Domain.Banking.Contracts;
using BT.Domain.HR.Contracts;
using BT.Domain.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> InDisplayOrder<T>(this IQueryable<T> query) where T : class, IOrderable 
        => query.OrderBy(x => x.DisplayOrder);
        
        
        
}
