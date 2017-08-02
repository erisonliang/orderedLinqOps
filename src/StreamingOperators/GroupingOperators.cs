using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StreamingOperators
{
    public static class GroupingOperators
    {
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified optional comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/>  whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys with.</param>
        /// <returns>A collection of elements of type TResult where each element represents a projection over a group and its key.</returns>
        public static IEnumerable<IGrouping<TKey, TSource>> OrderedGroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            return OrderedGroupByImpl(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified optional comparer.
        /// The elements of each group are projected by using a specified function.
        /// The keys are compared by using a specified optional comparer.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<TKey, TElement>> OrderedGroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer = null)
        {
            return OrderedGroupByImpl(source, keySelector, elementSelector, comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. 
        /// The keys are compared by using a specified optional comparer.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="resultSelector"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> OrderedGroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IComparer<TKey> comparer = null)
        {
            return OrderedGroupByImpl(source, keySelector, IdentityFunction<TSource>.Instance, comparer).Select(g => resultSelector(g.Key, g));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key.
        /// The elements of each group are projected by using a specified function.
        /// The keys are compared by using a specified optional comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each group.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <param name="source">A collection whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to map each source element to an element in a group.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>A collection of elements of type TResult where each element represents a projection over a group and its key.</returns>
        public static IEnumerable<TResult> OrderedGroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IComparer<TKey> comparer = null)
        {
            return OrderedGroupByImpl(source, keySelector, elementSelector, comparer).Select(g => resultSelector(g.Key, g)); ;
        }

        private static IEnumerable<IGrouping<TKey, TElement>> OrderedGroupByImpl<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null)
                comparer = Comparer<TKey>.Default;

            Grouping<TKey, TElement> grouping = null;

            foreach (var item in source)
            {
                var key = keySelector(item);

                // the first item just creates the grouping
                if (grouping == null)
                {
                    grouping = new Grouping<TKey, TElement>(key) { elementSelector(item) };
                }
                else
                {
                    // Each item is compared to the group key. When equal, it's added to the group. 
                    // When bigger, the previous (now complete) group is yielded and new one is created.
                    var comparisonResult = comparer.Compare(key, grouping.Key);
                    if (comparisonResult > 0)
                    {
                        yield return grouping;
                        grouping = new Grouping<TKey, TElement>(key) { elementSelector(item) };
                    }
                    else if (comparisonResult == 0)
                    {
                        grouping.Add(elementSelector(item));
                    }
                    else
                    {
                        throw new ArgumentException("The source collection is not ordered");
                    }
                }
            }

            // unless source collection was empty, there is one last group to yield
            if (grouping != null)
            {
                yield return grouping;
            }
        }

        private class Grouping<TKey, TSource> : IGrouping<TKey, TSource>
        {
            private readonly ICollection<TSource> collection = new List<TSource>();

            public Grouping(TKey key)
            {
                this.Key = key;
            }

            public TKey Key { get; }

            internal void Add(TSource item) => this.collection.Add(item);

            public IEnumerator<TSource> GetEnumerator() => this.collection.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable nonGeneric = this.collection;
                return nonGeneric.GetEnumerator();
            }
        }

        private static class IdentityFunction<T>
        {
            public static Func<T, T> Instance
            {
                get { return x => x; }
            }
        }
    }
}
