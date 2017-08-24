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

namespace Views
{
    /// <summary>
    /// Extensions for handling views.
    /// </summary>
    public class View
    {
        internal View() { }
        
        /// <summary>
        /// Rotates a crop box around it's centre.
        /// </summary>
        /// <param name="view">The view to rotate.</param>
        /// <param name="degrees">Rotation angle in degrees. Everything visible in the view will be rotated clockwise by this amount.</param>
        /// <returns>The rotated view.</returns>
        public static dynamic Rotate(Revit.Elements.Views.View view, double degrees)
        {

            var doc = DocumentManager.Instance.CurrentDBDocument;

            var iView = view.InternalElement as Autodesk.Revit.DB.View;

            Autodesk.Revit.DB.Element cropBoxElement = null;

            using( TransactionGroup tGroup = new TransactionGroup(doc))
            {
                tGroup.Start("Temp to find crop box element");

                using (Transaction t1 = new Transaction(doc, "Temp to find crop box element"))
                {

                    // Deactivate crop box
                    t1.Start();
                    iView.CropBoxVisible = false;
                    t1.Commit();

                    // Get all visible elements, which does not include the crop box
                    FilteredElementCollector collector = new FilteredElementCollector(doc, iView.Id);
                    ICollection<ElementId> shownElems = collector.ToElementIds();

                    // Activate the crop box
                    t1.Start();
                    iView.CropBoxVisible = true;
                    t1.Commit();

                    // Get all visible elements, excluding everything but the crop box
                    collector = new FilteredElementCollector(doc, iView.Id);
                    collector.Excluding(shownElems);

                    cropBoxElement = collector.FirstElement();

                }
                tGroup.RollBack();
            }
            
            using (Transaction t2 = new Transaction(doc))
            {
                var bBox = iView.CropBox;

                XYZ center = 0.5 * (bBox.Max + bBox.Min);
                var axis = Autodesk.Revit.DB.Line.CreateBound(center, center + XYZ.BasisZ);

                t2.Start("Rotate crop box element");
                ElementTransformUtils.RotateElement(doc, cropBoxElement.Id, axis, Math.PI * degrees / 180.0);
                t2.Commit();
            }
            
            return view;

        }
    }
}
