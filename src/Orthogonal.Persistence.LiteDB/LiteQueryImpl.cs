using System;
using LiteDB;

namespace Orthogonal.Persistence.LiteDB
{
    public class LiteQueryImpl<T> : LiteQuery<T>
    {

        public Func<ILiteQueryable<T>, ILiteQueryableResult<T>> Filter { get; }

        public LiteQueryImpl(Func<ILiteQueryable<T>, ILiteQueryableResult<T>> filter)
        {
            Filter = filter;
        }
    }
}