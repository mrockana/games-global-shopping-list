using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

public abstract class Specification<T>
    where T : BaseEntity
{
    protected Specification()
    {
        QueryModifiers = new Collection<Func<IQueryable<T>, IQueryable<T>>>();
        AndSpecifications = new Collection<Specification<T>>();
    }

    public bool IsPagination { get; set; }

    private bool WithNoTracking { get; set; }

    private Collection<Func<IQueryable<T>, IQueryable<T>>> QueryModifiers { get; }

    private Collection<Specification<T>> AndSpecifications { get; set; }

    public Specification<T> And(Specification<T> specification)
    {
        AndSpecifications.Add(specification);
        return this;
    }

    public Specification<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
    {
        QueryModifiers.Add(q => q.Include(path));
        return this;
    }

    public Specification<T> AsSplitQuery()
    {
        QueryModifiers.Add(q => q.AsSplitQuery());
        return this;
    }

    public void IgnoreAutoInclude<TProperty>(Expression<Func<T, TProperty>> path)
    {
        QueryModifiers.Add(q => q.IgnoreAutoIncludes());
    }

    public Specification<T> NoTracking()
    {
        WithNoTracking = true;
        return this;
    }

    public Specification<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> path)
    {
        QueryModifiers.Add(q => q.OrderBy(path));

        return this;
    }

    public Specification<T> WithPagination(int take, int skip)
    {
        IsPagination = true;

        QueryModifiers.Add(q => q.Skip(skip));
        QueryModifiers.Add(q => q.Take(take));

        return this;
    }

    public Specification<T> OrderByDescending<TProperty>(Expression<Func<T, TProperty>> path)
    {
        QueryModifiers.Add(q => q.OrderByDescending(path));

        return this;
    }

    public abstract Expression<Func<T, bool>> ToExpression();

    public Specification<T> WithQuery(Func<IQueryable<T>, IQueryable<T>> query)
    {
        QueryModifiers.Add(query);
        return this;
    }

    public IQueryable<T> GetQuery(IQueryable<T> query)
    {
        query = query.Where(ToExpression());

        if (AndSpecifications.Count > 0)
        {
            foreach (var andSpecification in AndSpecifications)
            {
                query = query.Where(andSpecification.ToExpression());
            }
        }

        if (QueryModifiers.Count > 0)
        {
            foreach (var modifier in QueryModifiers)
            {
                query = modifier.Invoke(query);
            }
        }

        if (WithNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}
