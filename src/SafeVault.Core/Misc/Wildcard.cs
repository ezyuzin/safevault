using System.Text.RegularExpressions;

namespace SafeVault.Misc
{
    public class Wildcard
    {
        public static bool Match(string pattern, string b)
        {
            return Match(pattern, b, false);
        }

        public static bool Match(string pattern, string b, bool ignoreCase)
        {
            RegexOptions opt = RegexOptions.Compiled;
            if (ignoreCase)
                opt = opt | RegexOptions.IgnoreCase;

            pattern = pattern.Replace("*", "**");
            pattern = pattern.Replace("?", "??");
            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace("\\*\\*", ".*");
            pattern = pattern.Replace("\\?\\?", ".");

            if (pattern.EndsWith(@"\..*"))
            {
                var pattern1 = pattern.Substring(0, pattern.Length - 4) + "$";
                if (Regex.IsMatch(b, pattern1, opt))
                    return true;
            }

            return Regex.IsMatch(b, pattern + "$", opt);
        }
    }
}