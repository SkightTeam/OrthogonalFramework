using System;
using System.Linq.Expressions;

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
}
