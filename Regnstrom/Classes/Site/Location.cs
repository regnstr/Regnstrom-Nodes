using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Revit.Elements;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Site
{
    /// <summary>
    /// Utility methods for handling true north and coordinates.
    /// </summary>
    public class Location
    {

        internal Location() { }

        /// <summary>
        /// Returns True North as a vector in the project coordinate system. The angle represents a right-handed rotation along the Z-axis to go from True to Project North.
        /// </summary>
        /// <returns></returns>
        [MultiReturn(new[] {"angle", "vector"})]
        public static Dictionary<string, dynamic> North()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            ProjectLocation projL = doc.ActiveProjectLocation;

            // THIS NEEDS TO BE UPDATED FOR 2018!
            ProjectPosition projP = projL.get_ProjectPosition(XYZ.Zero);

            return new Dictionary<string, dynamic>() {
                { "angle",  projP.Angle / Math.PI * 180.0 },
                { "vector", Vector.ByCoordinates(Math.Sin(projP.Angle), Math.Cos(projP.Angle), 0)}
            };
        }

    }
}