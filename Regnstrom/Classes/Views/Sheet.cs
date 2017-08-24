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

namespace Views
{
    public class Sheet
    {
        internal Sheet() { }
        

        public static dynamic[] PlacedViews(Revit.Elements.Views.Sheet sheet)
        {

            Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.ViewSheet iSheet = sheet.InternalElement as Autodesk.Revit.DB.ViewSheet;

            ISet<ElementId> eIds = iSheet.GetAllPlacedViews();

            List<dynamic> views = new List<dynamic>();
            foreach (ElementId eId in eIds)
            {
                views.Add(doc.GetElement(eId).ToDSType(true));
            }

            return views.ToArray();
        }

        /// <summary>
        /// Returns all scales present on a sheet in a sorted list of integers.
        /// </summary>
        /// <param name="sheet">The input sheet.</param>
        /// <returns></returns>
        public static int[] AllScales(Revit.Elements.Views.Sheet sheet)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.ViewSheet iSheet = sheet.InternalElement as Autodesk.Revit.DB.ViewSheet;

            ISet<ElementId> eIds = iSheet.GetAllPlacedViews();

            List<int> scales = new List<int>();

            scales.AddRange(eIds.Select(x => (doc.GetElement(x) as Autodesk.Revit.DB.View).Scale));
            
            scales = scales.Distinct().ToList();
            scales.Sort();

            return scales.ToArray();
        }

        [MultiReturn(new[] { "viewportNames", "viewportLocations"})]
        public static Dictionary<string, dynamic> ViewportLocations(Revit.Elements.Views.Sheet sheet)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            Autodesk.Revit.DB.ViewSheet iSheet = sheet.InternalElement as Autodesk.Revit.DB.ViewSheet;

            var allViewports = iSheet.GetAllViewports().Select(x => doc.GetElement(x) as Viewport);

            return new Dictionary<string, dynamic>() {
                { "viewportNames", allViewports.Select(x => (doc.GetElement(x.ViewId) as Autodesk.Revit.DB.View).ViewName)
                },
                { "viewportLocations", (
                    allViewports.Select(
                        x => x.GetBoxCenter()).Select(
                        x => Autodesk.DesignScript.Geometry.Point.ByCoordinates(x.X, x.Y, x.Z )))
                }
            };
        }
        

        public static bool AddView(Revit.Elements.Views.Sheet sheet, Revit.Elements.Views.View view, Autodesk.DesignScript.Geometry.Point point)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);

            ElementId sheetId = sheet.InternalElement.Id;
            ElementId viewId = view.InternalElement.Id;
            XYZ viewportLocation = new XYZ(point.X, point.Y, point.Z);

            Viewport.Create(doc, sheetId, viewId, viewportLocation);

            TransactionManager.Instance.TransactionTaskDone();
            
            return true;

        }
    }
}
