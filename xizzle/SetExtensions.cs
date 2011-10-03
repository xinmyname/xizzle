using System;
using System.Collections.Generic;

namespace xizzle
{
    internal static class SetExtensions
    {
        public static void Filter<T>(this HashSet<T> set, Predicate<T> match)
        {
            if (set != null) 
                set.RemoveWhere(e => !match(e));
        }
    }
}