using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingOperators
{
    public static class JoinOperators
    {
        public static IEnumerable<TResult> MergeJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
            where TKey : IComparable<TKey>
        {
            var comparer = Comparer<TKey>.Default;

            return MergeJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        public static IEnumerable<TResult> MergeJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, 
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, 
            Func<TOuter, TInner, TResult> resultSelector, IComparer<TKey> comparer)
        {
            if (outer == null) throw new ArgumentNullException(nameof(outer));
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            var innerOrdered = inner.OrderedSelect(innerKeySelector, (k, i) => new { Key = k, Value = i }, comparer);
            using (var innerIterator = innerOrdered.GetEnumerator())
            {
                var outerOrdered = outer.OrderedSelect(outerKeySelector, (k, i) => new { Key = k, Value = i }, comparer);
                using (var outerIterator = outerOrdered.GetEnumerator())
                {
                    if (!innerIterator.MoveNext() || !outerIterator.MoveNext())
                    {
                        yield break;
                    }

                    while (true)
                    {
                        var innerValue = innerIterator.Current.Value;
                        var innerKey = innerIterator.Current.Key;

                        var outerValue = outerIterator.Current.Value;
                        var outerKey = outerIterator.Current.Key;

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
                        else // must be equal
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

        public static IEnumerable<TResult> GroupMergeJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
            where TKey : IComparable<TKey>
        {
            var groupedInner = inner.GroupBy(innerKeySelector);

            return groupedInner.MergeJoin(outer, g => g.Key, outerKeySelector, (i, o) => resultSelector(o, i));
        }

        public static IEnumerable<TResult> GroupMergeJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IComparer<TKey> comparer)
        {
            var groupedInner = inner.GroupBy(innerKeySelector);

            return groupedInner.MergeJoin(outer, g => g.Key, outerKeySelector, (i, o) => resultSelector(o, i), comparer);
        }
    }
}
