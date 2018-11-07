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
    /// Utility methods for finding categores.
    /// </summary>
    public class Category
    {
        internal Category() { }

        /// <summary>
        /// Finds all categories matching the regex pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <returns></returns>
        public static List<string> FindByName(string pattern)
        {
            List<string> outputCategories = new List<string>();

            string[] categoryNames = Enum.GetNames(typeof(Autodesk.Revit.DB.Category));

            foreach (string s in categoryNames)
            {
                if (Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase))
                {
                    outputCategories.Add(s);
                }
            }

            return outputCategories;
        }

        /// <summary>
        /// Returns a list of all categories.
        /// </summary>
        /// <returns></returns>
        [MultiReturn(new[] { "ModelCategories", "AnnotationCategories"})]
        public static Dictionary<string, string[]> ListAll()
        {
            var categories = DocumentManager.Instance.CurrentDBDocument.Settings.Categories;
            List<string> modelCategories = new List<string>();
            List<string> annotationCategories = new List<string>();

            foreach (Autodesk.Revit.DB.Category c in categories)
            {
                if (c.CategoryType == CategoryType.Model)
                {
                    modelCategories.Add(c.Name);
                } else if (c.CategoryType == CategoryType.Annotation)
                {
                    annotationCategories.Add(c.Name);
                }
            }

            return new Dictionary<string, string[]>()
            {
                { "ModelCategories", modelCategories.ToArray() },
                { "AnnotationCategories", annotationCategories.ToArray() }
            };
        }
    }
}
