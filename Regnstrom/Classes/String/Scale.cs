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
    public class Scale
    {
        internal Scale() { }

        /// <summary>
        /// Converts a list of numbers into a string to be used on drawings.
        /// </summary>
        /// <param name="scaleValues"></param>
        /// <param name="spaces"></param>
        /// <returns></returns>
        [MultiReturn(new[] { "scale", "halfScale" })]
        public static Dictionary<string, string> ByInt(int[] scaleValues, bool spaces = false)
        {

            StringBuilder scale = new StringBuilder();
            StringBuilder halfScale = new StringBuilder();

            for (int i = 0; i < scaleValues.Length; i++ )
            {
                scale.Append("1" + (spaces ? " " : "") + ":" + (spaces ? " " : "") + scaleValues[i]);
                halfScale.Append("1" + (spaces ? " " : "") + ":" + (spaces ? " " : "") + scaleValues[i]*2);

                if(i<scaleValues.Length-1)
                {
                    scale.Append(", ");
                    halfScale.Append(", ");
                }

            }
            
            return new Dictionary<string, string>()
            {
                { "scale" , scale.ToString() },
                { "halfScale" , halfScale.ToString() }
            };

        }

    }
}
