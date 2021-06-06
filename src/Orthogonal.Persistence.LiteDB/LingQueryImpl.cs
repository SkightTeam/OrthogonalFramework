using System;
using System.Linq.Expressions;

namespace Orthogonal.Persistence.LiteDB
{
    public class LingQueryImpl<T> : LinqQuery<T>
    {
        public LingQueryImpl(Expression<Func<T, bool>> predicate)
        {
            Predicate = predicate;
        }

        public Expression<Func<T, bool>> Predicate { get; }
    }
}