using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;

namespace SmSimple.Core
{
    public static class Extender
    {
        public static string ToIcString(this int iValue)
        {
            return iValue.ToString(CultureInfo.InvariantCulture);
        }
        public static string ToIcString(this string sValue)
        {
            return sValue.ToString();
        }
        
        public static string ReplaceLfCr(this string sValue)
        {
            return sValue.Replace("\r", "<CR>").Replace("\n", "<LF>");
        }

        public static string ReplaceSohStxEtx(this string sValue)
        {
            const string SOH = "\x01";
            const string STX = "\x02";
            const string ETX = "\x03";
            return sValue.Replace(SOH, "<SOH>").Replace(STX, "<STX>").Replace(ETX, "<ETX>");
        }

        public static string ToStdDateTimeFormat(this DateTime dt)
        {
            return dt.ToString(DateTimeEx.FormatStringDefault, CultureInfo.InvariantCulture);
        }

        public static string ToStdFileDateTimeFormat(this DateTime dt)
        {
            return DateTimeEx.ToFileStringFormat(dt);
        }

        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
                                      TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }



        public static int StringCount(this string sValue, string subString)
        {
            int count = sValue.Length - sValue.Replace(subString, string.Empty).Length;
            return count;
        }

        public static int CharCount(this string sValue, char charToLookFor)
        {
            if (string.IsNullOrEmpty(sValue))
                return 0;

            // int count = sValue.Count(c => c == charToLookFor);
            int count = 0;
            char[] testchars = sValue.ToCharArray();
            int length = testchars.Length;
            for (int i = 0; i < length; i++)
            {
                if (testchars[i] == charToLookFor)
                    count++;
            }
            return count;
        }

        // Initialise array
        public static void Populate<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }


        // DistinctBy operator for LINQ
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
             (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }


        public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            // The tasck completion source. 
            var tcs = new TaskCompletionSource<bool>();

            // Register with the cancellation token.
            using (cancellationToken.Register(
                        s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                // If the task waited on is the cancellation token...
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);

            // Wait for one or the other to complete.
            return await task;
        }

    }
}
