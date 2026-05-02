using BT.Domain.Features.Banking.Contracts;
using BT.Domain.Features.HR.Contracts;
using BT.Domain.Features.IAM.Contracts;
using BT.Domain.Shared.Contracts;
using BT.Domain.Shared.Contracts.Common;
using BT.Domain.Features.Banking.Customers.Contracts.Repositories;
using BT.Domain.Features.HR.Employees.Contracts.Repositories;
using BT.Domain.Features.IAM.Users.Contracts.Repositories;
using BT.Domain.Shared.Contracts.Repositories;
using BT.Domain.Features.Banking.Customers.Contracts.Specifications;
using BT.Domain.Shared.Contracts.Specifications;

using BT.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace BT.Persistence.Contracts.Implementations.Repositories;

public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    private readonly DbContext _context = context;

    public async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        await _context.Set<T>().AddAsync(entity, ct).ConfigureAwait(false);
        return entity;
    }

    public virtual async Task<int> CountAsync(CancellationToken ct = default) => await _context.Set<T>().CountAsync(ct).ConfigureAwait(false);

    public virtual async Task<int> CountAsync<TCursor>(ISpecification<T, TCursor> spec, CancellationToken ct = default)
    {
        var query = _context.Set<T>().AsNoTracking();

        ArgumentNullException.ThrowIfNull(spec, nameof(spec));

        // Apply filters only — no ordering, no take, no includes
        if (spec.Criteria != null)
            query = spec.Criteria.Aggregate(query, (current, criteria) => current.Where(criteria));

        // Cursor filter is intentionally excluded — count should reflect total matching records, not records after the current page position

        return await query.CountAsync(ct).ConfigureAwait(false);
    }

    public async Task<T> DeleteAsync(Guid Id, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(Id, ct).ConfigureAwait(false);

        if (entity == null)
            throw new KeyNotFoundException($"Entity with id {Id} not found");

        _context.Set<T>().Remove(entity);
        return entity;
    }

    public async Task<T> SoftDeleteAsync(Guid Id, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(Id, ct).ConfigureAwait(false) ?? throw new KeyNotFoundException($"Entity with id {Id} not found");
        if (entity is ISoftDeletable softDeletableEntity) 
        {
            softDeletableEntity.IsDeleted = true;
            _context.Set<T>().Update(entity); 
        }
        else
        {
            throw new NotSupportedException($"Entity type {typeof(T).Name} does not support this method.");
        }
        return entity;
    }

    public async Task<T> DeleteAsync(string Id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Id cannot be null or empty", nameof(Id));

        if (!Guid.TryParse(Id, out var guidId))
            throw new ArgumentException("Id must be a valid GUID", nameof(Id));

        var entity = await FindByIdAsync(guidId, ct).ConfigureAwait(false);
        if (entity == null)
            throw new KeyNotFoundException($"Entity with id {Id} not found");
        _context.Set<T>().Remove(entity);
        return entity;
    }
    
    public async Task<T> SoftDeleteAsync(string Id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new ArgumentException("Id cannot be null or empty", nameof(Id));

        if (!Guid.TryParse(Id, out var guidId))
            throw new ArgumentException("Id must be a valid GUID", nameof(Id));

        var entity = await FindByIdAsync(guidId, ct).ConfigureAwait(false) ?? throw new KeyNotFoundException($"Entity with id {Id} not found");
        if (entity is ISoftDeletable softDeletableEntity) 
        {
            softDeletableEntity.IsDeleted = true;
            _context.Set<T>().Update(entity); 
        }
        else
        {
            throw new NotSupportedException($"Entity type {typeof(T).Name} does not support this method");
        }
        return entity;
    }

    public IQueryable<T> FindAll()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).AsNoTracking();

    }

    public async Task<T?> FindByIdAsync(Guid Id, CancellationToken ct = default)
    {
        return await _context.Set<T>().FindAsync([Id], ct).ConfigureAwait(false);

    }

    public async Task<Collection<T>> SearchAsync<TCursor>(ISpecification<T, TCursor> spec, CancellationToken ct = default)
    {
        var list = await _context.Set<T>().Specify(spec).AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
        //return new Collection<T>(list);
        return [..list];
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        //await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<int> UpdateRangeAsync(Collection<T> entities, CancellationToken ct = default)
    {
        if (entities == null || entities.Count == 0)
            return 0;

        _context.Set<T>().UpdateRange(entities);
        return await _context.SaveChangesAsync(ct).ConfigureAwait(false);


    }

    
}
