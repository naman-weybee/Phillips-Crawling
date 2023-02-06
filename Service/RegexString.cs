using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Phillips_Crawling_Task.Service
{
    public class RegexString
    {
        public static readonly Regex timeDurationRegex = new(@"(\d+)(\s?\,?\-?\&?\s?)((\d+)?(\s?\,?\-?\s?)(\w+)?(\s?\,?\-?\s?)(\d{4})?(((\s?\,?\-?\s?))(\d+)?(\s?\,?\-?\s?)(\d+)?(\s?\,?\-?\s?)(\w+)?(\s?\,?\-?\s?)(\d{4}))?)?", RegexOptions.IgnoreCase);
        public static readonly Regex uniqueIdRegex = new(@"(^/)?(\w+)$", RegexOptions.IgnoreCase);
        public static readonly Regex dimensionRegex = new(@"(\d+\.?\d+?)(\s?\,?\.?\-?\s?)(\w+)((\s?\,?\.?\-?\s?)(\w+)?(\s?\,?\.?\-?\s?)(\d+\.?\d+?)(\s?\,?\.?\-?\s?)(\w+))?", RegexOptions.IgnoreCase);
        public static readonly Regex priceRegex = new(@"^(\w+)?(\s?)(\w+)?(\s?)(\D*)(\d+)(\D*)$", RegexOptions.IgnoreCase);
        public static readonly Regex watchIdRegex = new(@"(\w+)?(\s?\-?\,?)(\d+)?(\s?\-?\,?)(\w+)?(\s?\-?\,?)(\d+)?", RegexOptions.IgnoreCase);
    }
}
