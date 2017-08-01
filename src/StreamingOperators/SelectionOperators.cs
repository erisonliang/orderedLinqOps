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

        public static IEnumerable<TResult> OrderedSelect<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TSource, TResult> resultSelector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            TKey previousKey = default(TKey);
            bool previousItemExists = false;
            TResult result = default(TResult);

            foreach (var item in source)
            {
                var key = keySelector(item);

                if (!previousItemExists)
                {
                    previousKey = key;
                    previousItemExists = true;
                    result = resultSelector(key, item);
                }
                else
                {
                    var comparisonResult = comparer.Compare(key, previousKey);
                    if (comparisonResult >= 0)
                    {
                        yield return result;
                        result = resultSelector(key, item);
                        previousKey = key;
                    }
                    else
                    {
                        throw new ArgumentException("The source collection is not ordered");
                    }
                }
            }

            if (previousItemExists)
            {
                yield return result;
            }
        }
    }
}
