using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Revit.Elements;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

using RevitServices.Persistence;
using RevitServices.Transactions;

namespace String
{
    /// <summary>
    /// Contains some of the static methods of the C# Regex class adapted to Dynamo.
    /// </summary>
    public class Regex
    {
        internal Regex() { }

        /// <summary>
        /// Checks if the regular expression matches the input.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns></returns>
        public static bool IsMatch(string input, string pattern)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// Finds the first match of the regular expression.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "success", "captures" })]
        public static Dictionary<string, dynamic> Match(string input, string pattern)
        {
            Match match = System.Text.RegularExpressions.Regex.Match(input, pattern);

            List<dynamic> captures = new List<dynamic>();
            
            foreach (System.Text.RegularExpressions.Group group in match.Groups)
            {
                List<string> tempCaptures = new List<string>();
                foreach (Capture capture in group.Captures)
                {
                    tempCaptures.Add(capture.Value);
                }
                captures.Add(tempCaptures);
            }


            return new Dictionary<string, dynamic>()
            {
                { "success", match.Success },
                { "captures", captures }
            };
        }

        /// <summary>
        /// Finds all matches of the regular expression.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="pattern">The regex pattern.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "matches", "captures" })]
        public static Dictionary<string, dynamic> Matches(string input, string pattern)
        {
            MatchCollection matchCollection = System.Text.RegularExpressions.Regex.Matches(input, pattern);
            
            List<dynamic> captures = new List<dynamic>();

            foreach (Match match in matchCollection)
            {
                List<dynamic> tempMatches = new List<dynamic>();
                foreach (System.Text.RegularExpressions.Group group in match.Groups)
                {
                    List<string> tempCaptures = new List<string>();
                    foreach (Capture capture in group.Captures)
                    {
                        tempCaptures.Add(capture.Value);
                    }
                    tempMatches.Add(tempCaptures);
                }
                captures.Add(tempMatches);
            }

            return new Dictionary<string, dynamic>() {
                {"matches", matchCollection.Count},
                {"captures", captures}
            };
            
        }

        /// <summary>
        /// Replaces all matches in "input" with "replacement".
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace(string input, string pattern, string replacement)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement);
        }

        /// <summary>
        /// Returns substrings of the input, split where the regex pattern matches.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string[] Split(string input, string pattern)
        {
            return System.Text.RegularExpressions.Regex.Split(input, pattern);
        }
    }
}
