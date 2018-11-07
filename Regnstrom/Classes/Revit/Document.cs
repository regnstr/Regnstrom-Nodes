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
    /// Always overwrite families, never overwrite parameter values
    /// </summary>
    class FamilyOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = false;
            return true;
        }

        public bool OnSharedFamilyFound(
            Autodesk.Revit.DB.Family sharedFamily,
            bool familyInUse,
            out FamilySource source,
            out bool overwriteParameterValues)
        {
            overwriteParameterValues = false;
            source = FamilySource.Family;
            return true;
        }
    }

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

        /// <summary>
        /// Loads a family at the specified path into the current document. Will always overwrite existing families and will never overwrite parameter values.
        /// </summary>
        /// <param name="path">File path of the family to load.</param>
        /// <returns>The family name of the loaded family.</returns>
        public static string LoadFamily(string path)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Family outFamily = null;

            /*
            using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(doc))
            {
                if (transaction.Start("Load family into document") == TransactionStatus.Started)
                {
                    doc.LoadFamily(path, new FamilyOptions(), out outFamily);

                    if (transaction.Commit() != TransactionStatus.Committed)
                    {
                        transaction.RollBack();
                    }
                }
            }*/

            TransactionManager.Instance.EnsureInTransaction(doc);
            
            doc.LoadFamily(path, new FamilyOptions(), out outFamily);

            TransactionManager.Instance.TransactionTaskDone();
            
            return outFamily.Name;
        }

        /// <summary>
        /// Loads one ore more family documents into a specified family document.
        /// </summary>
        /// <param name="pathToContainerFamily">Path to the document into which the families will be loaded.</param>
        /// <param name="pathToFamiliesToLoad">Path to the families to load.</param>
        /// <returns></returns>
        public static bool LoadFamilyIntoFamilyDocument(string pathToContainerFamily, string[] pathToFamiliesToLoad)
        {

            Autodesk.Revit.DB.Document familyDoc = null;

            var app = DocumentManager.Instance.CurrentUIApplication.Application;

            familyDoc = app.OpenDocumentFile(pathToContainerFamily);

            if (!familyDoc.IsFamilyDocument)
            {
                throw new ArgumentException("The specified document is not a family document (.rfa)");
            }
            
            using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(familyDoc))
            {
                if (transaction.Start("Load family into family") == TransactionStatus.Started)
                {

                    for (int i = 0; i < pathToFamiliesToLoad.Length; i++)
                    {
                        familyDoc.LoadFamily(pathToFamiliesToLoad[i]);
                    }

                    if (TransactionStatus.Committed != transaction.Commit())
                    {
                        transaction.RollBack();
                    }
                }
            }

            // The Close() method saves the document
            familyDoc.Close();

            return true;

        }

    }
}
