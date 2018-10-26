using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// RevitAPI
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

// Dynamo Core
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

// Dynamo Revit
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.GeometryConversion;

namespace Regnstrom.Annotations
{
    public static class FilledRegion
    {
        [MultiReturn(new[] { "InActiveView", "InProject" })]
        public static Dictionary<string, dynamic> GetAll()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var inView = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(Autodesk.Revit.DB.FilledRegion)).ToElements();
            var inProject = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.FilledRegion)).ToElements();
            
            var inViewDS = new List<dynamic>();
            foreach (var i in inView)
            {
                inViewDS.Add(i.ToDSType(true));
            }

            var inProjectDS = new List<dynamic>();
            foreach (var j in inProject)
            {
                inProjectDS.Add(j.ToDSType(true));
            }

            return new Dictionary<string, dynamic>
            {
                {"InActiveView", inViewDS },
                {"InProject", inProjectDS }
            };
        }

        public static dynamic BoundaryCurves(Revit.Elements.FilledRegion filledRegion)
        {

            var iElement = filledRegion.InternalElement as Autodesk.Revit.DB.FilledRegion;
            var cLoop = iElement.GetBoundaries();

            var returnList = new List<dynamic>();

            foreach(CurveLoop cl in cLoop)
            {
                var tempList = new List<dynamic>();
                returnList.Add(tempList);

                foreach(Autodesk.Revit.DB.Curve c in cl)
                {
                    tempList.Add(c.ToProtoType());
                }
            }

            return returnList;

        }
    }
}
