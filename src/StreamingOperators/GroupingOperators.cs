using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StreamingOperators
{
    /// <summary>
    /// Implements the GroupBy operators
    /// </summary>
    public static class GroupingOperators
    {
        /// <summary>
        /// Groups the elements of an ordered sequence according to a specified key selector function, using a specified optional comparer.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the input is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A collection whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>A collection where each object contains a sequence of objects and a key.</returns>
        /// <exception cref="ArgumentException">The input sequences is out of order.</exception>
        public static IEnumerable<IGrouping<TKey, TSource>> OrderedGroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer = null)
        {
            return OrderedGroupBy(source, keySelector, IdentityFunction<TSource>.Instance, CreateGrouping, comparer);
        }

        /// <summary>
        /// Groups the elements of an ordered sequence according to a specified key selector function, using a specified optional comparer.
        /// The elements of each group are projected by using a specified function.
        /// The keys are compared by using a specified optional comparer.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the input is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each group.</typeparam>
        /// <param name="source">A collection whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to extract the key for each element.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>A collection where each object contains a sequence of objects and a key.</returns>
        /// <exception cref="ArgumentException">The input sequences is out of order.</exception>
        public static IEnumerable<IGrouping<TKey, TElement>> OrderedGroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector, IComparer<TKey> comparer = null)
        {
            return OrderedGroupBy(source, keySelector, elementSelector, CreateGrouping, comparer);
        }

        /// <summary>
        /// Groups the elements of an ordered sequence according to a specified key selector function and creates a result value from each group and its key. 
        /// The keys are compared by using a specified optional comparer.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the input is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <param name="source">A collection whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to map each source element to an element in a group.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>A collection of elements of type TResult where each element represents a projection over a group and its key.</returns>
        /// <exception cref="ArgumentException">The input sequences is out of order.</exception>
        public static IEnumerable<TResult> OrderedGroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IComparer<TKey> comparer = null)
        {
            return OrderedGroupBy(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
        }

        /// <summary>
        /// Groups the elements of an ordered sequence according to a specified key selector function and creates a result value from each group and its key.
        /// The elements of each group are projected by using a specified function.
        /// The keys are compared by using a specified optional comparer.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the input is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
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
        /// <exception cref="ArgumentException">The input sequences is out of order.</exception>
        private static IEnumerable<TResult> OrderedGroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, 
            Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (comparer == null)
                comparer = Comparer<TKey>.Default;

            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }

                // for the first item, there is nothing to compare it to, so we only extract the key and create the first group
                var item = iterator.Current;
                var grouping = (key: keySelector(item), list: new List<TElement> { elementSelector(item) });

                while (iterator.MoveNext())
                {
                    // Each item is compared to the group key. When equal, it's added to the group. 
                    // When bigger, the previous (now complete) group is yielded and new one is created.
                    item = iterator.Current;
                    var key = keySelector(item);
                    var comparisonResult = comparer.Compare(key, grouping.key);
                    if (comparisonResult > 0)
                    {
                        yield return resultSelector(grouping.key, grouping.list);
                        grouping = (key, new List<TElement> { elementSelector(item) });
                    }
                    else if (comparisonResult == 0)
                    {
                        grouping.list.Add(elementSelector(item));
                    }
                    else
                    {
                        throw new ArgumentException("The source collection is not ordered");
                    }
                }

                yield return resultSelector(grouping.key, grouping.list);
            }
        }

        private static Grouping<TKey, TElement> CreateGrouping<TKey, TElement>(TKey key, IEnumerable<TElement> collection)
        {
            return new Grouping<TKey, TElement>(key, collection);
        }

        private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            private readonly IEnumerable<TElement> collection;

            public Grouping(TKey key, IEnumerable<TElement> collection)
            {
                this.Key = key;
                this.collection = collection;
            }

            public TKey Key { get; }

            public IEnumerator<TElement> GetEnumerator() => this.collection.GetEnumerator();

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
