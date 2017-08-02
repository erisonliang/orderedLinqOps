using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamingOperators
{
    public static class JoiningOperators
    {
        public static IEnumerable<TResult> OrderedJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
            where TKey : IComparable<TKey>
        {
            var comparer = Comparer<TKey>.Default;

            return OrderedJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        public static IEnumerable<TResult> OrderedJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector, IComparer<TKey> comparer)
        {
            if (outer == null) throw new ArgumentNullException(nameof(outer));
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            var innerOrdered = inner.OrderedSelect(innerKeySelector, (k, i) => (k, i), comparer);
            using (var innerIterator = innerOrdered.GetEnumerator())
            {
                var outerOrdered = outer.OrderedSelect(outerKeySelector, (k, i) => (k, i), comparer);
                using (var outerIterator = outerOrdered.GetEnumerator())
                {
                    if (!innerIterator.MoveNext() || !outerIterator.MoveNext())
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
