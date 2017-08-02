using System;
using System.Collections.Generic;

namespace StreamingOperators
{
    public static class SelectionOperators
    {
        /// <summary>
        /// Projects each element of an ordered sequence into a new form, while checking the correctness of the ordering.
        /// </summary>
        /// <remarks>
        /// The operation works in a "streaming" way, meaning the input is not buffered, but passed along as soon as possible.
        /// The ordering of the input is assumed to be compatible with the provided comparer. An exception will be thrown during the iteration if this assumption is not upheld.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A transform function to apply to each element.</param>
        /// <param name="comparer">A "sorting" comparer to compare keys with.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> whose elements are the result of invoking the transform function on each element of source.</returns>
        public static IEnumerable<TResult> OrderedSelect<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, 
            Func<TKey, TSource, TResult> resultSelector, IComparer<TKey> comparer = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            if (comparer == null)
                comparer = Comparer<TKey>.Default;

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
