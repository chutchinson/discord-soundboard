using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Soundboard.Text
{
    public static class StringExtensions
    {
        /// <summary>
        /// Applies pluralization for English words based on the number
        /// of items specified. 
        /// 
        /// NOTE: This is extremely naive and is adjusted for a very specific use case.
        /// </summary>
        /// <param name="word">word to pluralize</param>
        /// <param name="count">plurality</param>
        /// <returns></returns>
        public static string Pluralize(this string str, long count)
        {
            if (count > 1)
                return str.EndsWith("s") ? str : str + "s";

            return str;
        }
    }
}
