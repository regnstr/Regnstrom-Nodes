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
    /// Some extension methods related to the open document.
    /// </summary>
    public class Document
    {
        internal Document() { }

        /// <summary>
        /// Returns a list of all predefined dwg export options.
        /// </summary>
        /// <returns></returns>
        public static string[] DWGExportOptions()
        {
            return ExportDWGSettings.ListNames(DocumentManager.Instance.CurrentDBDocument).ToArray();
        }

        /// <summary>
        /// Returns all views in the active document except view templates.
        /// </summary>
        /// <param name="toggle"></param>
        /// <returns></returns>
        public static dynamic[] AllViews(bool toggle = false)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var collector = new FilteredElementCollector(doc);

            var elements = collector.OfClass(typeof(View)).ToElements().Select(
                    x => x as View
                ).Where(
                    x => !(x.IsTemplate || x.ViewType == ViewType.Undefined || x.ViewType == ViewType.Internal || x.ViewType == ViewType.ProjectBrowser || x.ViewType == ViewType.SystemBrowser || x.ViewType == ViewType.DrawingSheet )
                ).Select(
                    x => x.ToDSType(true)
                );

            return elements.ToArray();
        }

        /// <summary>
        /// Returns all sheets in the active document.
        /// </summary>
        /// <param name="toggle"></param>
        /// <returns></returns>
        public static dynamic[] AllSheets(bool toggle = false)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var collector = new FilteredElementCollector(doc);

            var elements = collector.OfClass(typeof(View)).ToElements().Select(
                    x => x as View
                ).Where(
                    x => !(x.IsTemplate || x.ViewType != ViewType.DrawingSheet)
                ).Select(
                    x => x.ToDSType(true)
                );

            return elements.ToArray();
        }
    }
}
