using System;
using LiteDB;

namespace Orthogonal.Persistence.LiteDB
{
    public interface LiteQuery<T> : Query<T>
    {
        Func<ILiteQueryable<T>, ILiteQueryableResult<T>> Filter { get; }
    }
}