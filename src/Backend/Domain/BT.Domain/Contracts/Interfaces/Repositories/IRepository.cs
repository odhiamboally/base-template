using BT.Domain.Contracts.Specifications;
using BT.Domain.Banking.Entities;
using BT.Domain.HR.Entities;
using BT.Domain.IAM.Entities;
using BT.Domain.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace BT.Domain.Contracts.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountAsync<TCursor>(ISpecification<T, TCursor> spec, CancellationToken ct = default);
    Task<T> DeleteAsync(Guid Id, CancellationToken ct = default);
    Task<T> SoftDeleteAsync(Guid Id, CancellationToken ct = default);
    Task<T> DeleteAsync(string Id, CancellationToken ct = default);
    Task<T> SoftDeleteAsync(string Id, CancellationToken ct = default);
    IQueryable<T> FindAll();
    Task<T?> FindByIdAsync(Guid Id, CancellationToken ct = default);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
    Task<Collection<T>> SearchAsync<TCursor>(ISpecification<T, TCursor> spec, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity);
    Task<int> UpdateRangeAsync(Collection<T> entities, CancellationToken ct = default);

}
