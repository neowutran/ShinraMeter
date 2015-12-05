using System;
using System.Collections.Generic;

namespace Tera.Game
{
    internal static class Helpers
    {
        public static void On<T>(this object obj, Action<T> callback)
        {
            if (obj is T)
            {
                var castObject = (T) obj;
                callback(castObject);
            }
        }

        public class ProjectingEqualityComparer<T, TKey> : Comparer<T>
        {
            private readonly Comparer<TKey> _keyComparer = Comparer<TKey>.Default;
            private readonly Func<T, TKey> _projection;

            public ProjectingEqualityComparer(Func<T, TKey> projection)
            {
                _projection = projection;
            }

            public override int Compare(T x, T y)
            {
                return _keyComparer.Compare(_projection(x), _projection(y));
            }
        }
    }
}