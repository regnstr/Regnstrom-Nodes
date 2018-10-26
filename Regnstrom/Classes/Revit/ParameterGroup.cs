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

namespace Revit
{
    /// <summary>
    /// Utility methods for finding builtin parameter groups.
    /// </summary>
    public class ParameterGroup
    {
        internal ParameterGroup() { }

        /// <summary>
        /// Finds all builtin parameter group names matching the regex pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <returns></returns>
        public static List<string> FindByName(string pattern)
        {
            List<string> outputParameterGroupNames = new List<string>();

            string[] parameterGroupNames = Enum.GetNames(typeof(Autodesk.Revit.DB.BuiltInParameterGroup));

            foreach (string s in parameterGroupNames)
            {
                if(Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase))
                {
                    outputParameterGroupNames.Add(s);
                }
            }

            return outputParameterGroupNames;
        }

        /// <summary>
        /// Returns a list of all builtin parameter groups.
        /// </summary>
        /// <returns></returns>
        public static string[] ListAll()
        {
            return Enum.GetNames(typeof(Autodesk.Revit.DB.BuiltInParameterGroup));
        }
    }
}
