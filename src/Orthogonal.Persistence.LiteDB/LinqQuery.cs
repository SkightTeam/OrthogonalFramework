using System;
using System.Linq.Expressions;

namespace Orthogonal.Persistence.LiteDB
{
    public interface LinqQuery<T> : Query<T>
    {
        Expression<Func<T, bool>> Predicate { get; }
    }
}
