using BT.Domain.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Persistence.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> InDisplayOrder<T>(this IQueryable<T> query) where T : class, IOrderable 
        => query.OrderBy(x => x.DisplayOrder);
        
        
        
}
