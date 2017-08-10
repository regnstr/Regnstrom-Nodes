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
    /// Utility methods for finding builtin categores.
    /// </summary>
    public class BuiltInCategory
    {
        internal BuiltInCategory() { }

        /// <summary>
        /// Finds all builtin categories matching the regex pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <returns></returns>
        public static List<string> FindByName(string pattern)
        {
            List<string> outputCategories = new List<string>();

            string[] categoryNames = Enum.GetNames(typeof(Autodesk.Revit.DB.BuiltInCategory));

            foreach (string s in categoryNames)
            {
                if(Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase))
                {
                    outputCategories.Add(s);
                }
            }

            return outputCategories;
        }

        /// <summary>
        /// Returns a list of all builtin categories.
        /// </summary>
        /// <returns></returns>
        public static string[] ListAll()
        {
            return Enum.GetNames(typeof(Autodesk.Revit.DB.BuiltInCategory));
        }
    }
}
