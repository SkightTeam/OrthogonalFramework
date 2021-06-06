using System;
using System.Linq.Expressions;
using LiteDB;

namespace Orthogonal.Persistence.LiteDB
{
    public interface LinqQuery<T> : Query<T>
    {
        Expression<Func<T, bool>> Predicate { get; }
    }

    public class LingQueryImpl<T> : LinqQuery<T>
    {
        public LingQueryImpl(Expression<Func<T, bool>> predicate)
        {
            Predicate = predicate;
        }

        public Expression<Func<T, bool>> Predicate { get; }
    }

    public interface LiteQuery<T> : Query<T>
    {
        Func<ILiteQueryable<T>, ILiteQueryableResult<T>> Filter { get; }
    }

    public class LiteQueryImpl<T> : LiteQuery<T>
    {

        public Func<ILiteQueryable<T>, ILiteQueryableResult<T>> Filter { get; }

        public LiteQueryImpl(Func<ILiteQueryable<T>, ILiteQueryableResult<T>> filter)
        {
            Filter = filter;
        }
    }
}
