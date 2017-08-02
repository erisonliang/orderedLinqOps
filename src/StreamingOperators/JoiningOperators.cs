using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamingOperators
{
    public static class JoiningOperators
    {
        /// <summary>
        /// Correlates the elements of two ordered sequences based on matching keys, using a specified optional comparer.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the inputs is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>A collection that has elements of type TResult that are obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="ArgumentException">Any of the input sequences is out of order.</exception>
        public static IEnumerable<TResult> OrderedJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector, IComparer<TKey> comparer = null)
        {
            if (outer == null) throw new ArgumentNullException(nameof(outer));
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null)
                comparer = Comparer<TKey>.Default;

            var innerOrdered = inner.OrderedSelect(innerKeySelector, (k, i) => (k, i), comparer);
            using (var innerIterator = innerOrdered.GetEnumerator())
            {
                if (!innerIterator.MoveNext())
                {
                    yield break;
                }

                var outerOrdered = outer.OrderedSelect(outerKeySelector, (k, i) => (k, i), comparer);
                using (var outerIterator = outerOrdered.GetEnumerator())
                {
                    if (!outerIterator.MoveNext())
                    {
                        yield break;
                    }

                    while (true)
                    {
                        (var innerKey, var innerValue) = innerIterator.Current;
                        (var outerKey, var outerValue) = outerIterator.Current;

                        var comparisonResult = comparer.Compare(innerKey, outerKey);

                        if (comparisonResult < 0)
                        {
                            // advance inner
                            if (!innerIterator.MoveNext())
                            {
                                yield break;
                            }
                        }
                        else if (comparisonResult > 0)
                        {
                            // advance outer
                            if (!outerIterator.MoveNext())
                            {
                                yield break;
                            }
                        }
                        else // comparisonResult == 0
                        {
                            yield return resultSelector(outerValue, innerValue);

                            // advance inner
                            if (!innerIterator.MoveNext())
                            {
                                yield break;
                            }
                        }
                    }
                }
            }
        }

        //public static IEnumerable<TResult> GroupMergeJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
        //    IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
        //    Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        //    where TKey : IComparable<TKey>
        //{
        //    var groupedInner = inner.GroupBy(innerKeySelector);

        //    return groupedInner.OrderedJoin(outer, g => g.Key, outerKeySelector, (i, o) => resultSelector(o, i));
        //}

        //public static IEnumerable<TResult> GroupMergeJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
        //    IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
        //    Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IComparer<TKey> comparer)
        //{
        //    var groupedInner = inner.GroupBy(innerKeySelector);

        //    return groupedInner.OrderedJoin(outer, g => g.Key, outerKeySelector, (i, o) => resultSelector(o, i), comparer);
        //}
    }
}
