using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace zAppDev.DotNet.Framework.Utilities
{
    public static class RegExHelper
    {
        private static Match CalculateMatch(CSharpVerbalExpressions.VerbalExpressions verbalExpression, string input)
        {
            var pattern = verbalExpression.ToRegex()?.ToString();

            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new Exception("Could not parse the VerbalExpression into a string pattern.");
            }

            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);

            return match;
        }

        public static string GetMatch(CSharpVerbalExpressions.VerbalExpressions verbalExpression, string input, string defaultValue = null)
        {
            try
            {
                var match = CalculateMatch(verbalExpression, input);

                if (match.Success)
                {
                    return match.Value;
                }

                return defaultValue;
            }
            catch (Exception)
            {
                if(defaultValue != null)
                {
                    return defaultValue;
                }

                throw;
            }
        }//end GetMatch()

        public static List<string> GetMatches(CSharpVerbalExpressions.VerbalExpressions verbalExpression, string input, List<string> defaultValues = null)
        {
            try
            {
                var result = new List<string>();
                var match = CalculateMatch(verbalExpression, input);

                if (match.Success)
                {
                    foreach (var capture in match.Captures)
                    {
                        result.Add(capture.ToString());
                    }
                    return result;
                }

                return defaultValues;
            }
            catch (Exception)
            {
                if (defaultValues != null)
                {
                    return defaultValues;
                }

                throw;
            }
        }//end GetMatches()
    }
}
