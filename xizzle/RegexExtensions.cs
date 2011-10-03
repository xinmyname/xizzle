using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace xizzle
{
    internal static class RegexExtensions
    {
        public static IEnumerable<Capture> SubcapturesOf(this Group g, Capture c)
        {
            return g.Captures.Cast<Capture>().Where(x => x.Index >= c.Index && x.Index + x.Length <= c.Index + c.Length);
        }
    }
}