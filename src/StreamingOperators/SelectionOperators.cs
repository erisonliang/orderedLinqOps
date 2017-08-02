using System;
using System.Collections.Generic;

namespace StreamingOperators
{
    public static class SelectionOperators
    {
        public static IEnumerable<TResult> OrderedSelect<TSource, TKey, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TSource, TResult> resultSelector)
            where TKey : IComparable<TKey>
        {
            var comparer = Comparer<TKey>.Default;

            return source.OrderedSelect(keySelector, resultSelector, comparer);
        }

        public static IEnumerable<TResult> OrderedSelect<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, 
            Func<TKey, TSource, TResult> resultSelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            using (var iterator = source.GetEnumerator())
            {
                // for the first item, there is nothing to compare it to, so we only extract the key
                if (!iterator.MoveNext())
                {
                    yield break;
                }

                var item = iterator.Current;
                var previousKey = keySelector(item); ;
                yield return resultSelector(previousKey, item);

                // for all the other items, we compare the current key to the previous one
                while (iterator.MoveNext())
                {
                    item = iterator.Current;
                    var key = keySelector(item);

                    var comparisonResult = comparer.Compare(key, previousKey);
                    if (comparisonResult >= 0)
                    {
                        yield return resultSelector(key, item);
                        previousKey = key;
                    }
                    else
                    {
                        throw new ArgumentException("The source collection is not ordered");
                    }
                }
            }
        }
    }
}
