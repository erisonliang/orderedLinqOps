using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StreamingOperators
{
    public static class GroupingOperators
    {
        public static IEnumerable<IGrouping<TKey, TSource>> OrderedGroupBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            where TKey : IComparable<TKey>
        {
            var comparer = Comparer<TKey>.Default;

            return source.OrderedGroupBy(keySelector, comparer);
        }

        private static IEnumerable<IGrouping<TKey, TSource>> OrderedGroupBy<TSource, TKey>(this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            Grouping<TKey, TSource> grouping = null;

            foreach (var item in source)
            {
                var key = keySelector(item);

                if (grouping == null)
                {
                    grouping = new Grouping<TKey, TSource>(key, new List<TSource> { item });
                }
                else
                {
                    var comparisonResult = comparer.Compare(key, grouping.Key);
                    if (comparisonResult > 0)
                    {
                        yield return grouping;
                        grouping = new Grouping<TKey, TSource>(key, new List<TSource> { item });
                    }
                    else if (comparisonResult == 0)
                    {
                        grouping.Add(item);
                    }
                    else
                    {
                        throw new ArgumentException("The source collection is not ordered");
                    }
                }
            }

            if (grouping != null)
            {
                yield return grouping;
            }
        }

        private class Grouping<TKey, TSource> : IGrouping<TKey, TSource>
        {
            private readonly ICollection<TSource> collection;

            public Grouping(TKey key, ICollection<TSource> collection)
            {
                this.collection = collection;
                this.Key = key;
            }

            internal void Add(TSource item)
            {
                this.collection.Add(item);
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                return this.collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this.collection).GetEnumerator();
            }

            public TKey Key { get; }
        }
    }
}
