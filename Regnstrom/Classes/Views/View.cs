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
        /// Rotates a view around its center point. It is equivalent to selecting the crop region and rotating it counter clockwise.
        /// </summary>
        /// <param name="view">The view to rotate.</param>
        /// <param name="degrees">Rotation angle in degrees. Everything visible in the view will be rotated clockwise by this amount.</param>
        /// <returns>The rotated view.</returns>
        public static Revit.Elements.Views.View Rotate(Revit.Elements.Views.View view, double degrees)
        {

            // Setup
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var iView = view.InternalElement as Autodesk.Revit.DB.View;
            ViewCrop viewCrop = GetViewCrop(iView);
            
            // Rotate the crop element
            RotateCropElement(iView, viewCrop.cropElement, degrees);

            return view;

        }

        /// <summary>
        /// Sets the rotation angle of a view around its center point. It is equivalent to resetting its rotation and then selecting the crop region and rotating it counter clockwise.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Revit.Elements.Views.View SetRotation(Revit.Elements.Views.View view, double degrees)
        {

            // Setup
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var iView = view.InternalElement as Autodesk.Revit.DB.View;
            ViewCrop viewCrop = GetViewCrop(iView);

            // Start by resetting the rotation
            ResetRotation(iView);

            // Rotate the crop element
            RotateCropElement(iView, viewCrop.cropElement, degrees);
            
            return view;

        }



        /// <summary>
        /// Copies the crop region and rotation of one view and applies it to another. Useful for creating views with identical cropping at different levels in a project.
        /// </summary>
        /// <param name="source">The source view.</param>
        /// <param name="target">The target view.</param>
        /// <returns></returns>
        public static Revit.Elements.Views.View CopyCrop(Revit.Elements.Views.View source, Revit.Elements.Views.View target)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var iSourceView = source.InternalElement as Autodesk.Revit.DB.View;
            var iTargetView = target.InternalElement as Autodesk.Revit.DB.View;

            var sourceCropRegion = GetViewCrop(iSourceView).cropRegion;

            TransactionManager.Instance.EnsureInTransaction(doc);

            SetRotation(target, GetRotation(source));

            iTargetView.CropBoxVisible = true;
            iTargetView.GetCropRegionShapeManager().SetCropShape(CurveLoop.CreateViaCopy(sourceCropRegion));

            // SUPER IMPORTANT TO REGENERATE THE DOCUMENT AFTER THE WORK IS DONE!
            DocumentManager.Regenerate();
            TransactionManager.Instance.TransactionTaskDone();

            return target;
           
        }

        /// <summary>
        /// Returns the crop element for the view.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Revit.Elements.Element GetCropElement(Revit.Elements.Views.View view)
        {
            return GetViewCrop(view.InternalElement as Autodesk.Revit.DB.View).cropElement.ToDSType(true);
        }

        /// <summary>
        /// Get the rotation of the crop element of a view. The rotation is expressed in degrees, counter clockwise.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static double GetRotation(Revit.Elements.Views.View view)
        {
            XYZ rightDirection = (view.InternalElement as Autodesk.Revit.DB.View).RightDirection;

            int sign = 1;

            if (rightDirection.Y<0)
            {
                sign = -1;
            }

            return rightDirection.AngleTo(XYZ.BasisX) * 180.0 / Math.PI * sign;

        }

        #region Helpers

        /// <summary>
        /// Struct to hold the crop element and crop region for a view
        /// </summary>
        private struct ViewCrop
        {
            public Autodesk.Revit.DB.Element cropElement;
            public Autodesk.Revit.DB.CurveLoop cropRegion;
        }

        /// <summary>
        /// Resets the rotation of a view by setting its orientation to true north and then back to project north.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        private static bool ResetRotation(Autodesk.Revit.DB.View view)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            view.get_Parameter(BuiltInParameter.PLAN_VIEW_NORTH).Set(1);
            view.get_Parameter(BuiltInParameter.PLAN_VIEW_NORTH).Set(0);
            TransactionManager.Instance.TransactionTaskDone();

            return true;
        }


        /// <summary>
        /// Rotates the crop region of a view by the amount specified
        /// </summary>
        /// <param name="view"></param>
        /// <param name="cropElement"></param>
        /// <param name="degrees"></param>
        /// <returns></returns>
        private static bool RotateCropElement(Autodesk.Revit.DB.View view, Autodesk.Revit.DB.Element cropElement, double degrees)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            
            var bBox = view.CropBox;

            XYZ center = 0.5 * (bBox.Max + bBox.Min);
            var axis = Autodesk.Revit.DB.Line.CreateBound(center, center + XYZ.BasisZ);
            
            ElementTransformUtils.RotateElement(doc, cropElement.Id, axis, Math.PI * degrees / 180.0);

            TransactionManager.Instance.TransactionTaskDone();

            return true;
        }

        /// <summary>
        /// Get the view crop element and crop region curves
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        private static ViewCrop GetViewCrop(Autodesk.Revit.DB.View view)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);

            ViewCrop viewCrop;

            using (SubTransaction tGroup = new SubTransaction(doc))
            {
                tGroup.Start();

                using (SubTransaction t1 = new SubTransaction(doc))
                {

                    // Deactivate crop box
                    t1.Start();
                    view.CropBoxVisible = false;
                    t1.Commit();
                    doc.Regenerate();

                    // Get all visible elements, which does not include the crop box
                    FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
                    ICollection<ElementId> shownElems = collector.ToElementIds();

                    // Activate the crop box
                    t1.Start();
                    view.CropBoxVisible = true;
                    t1.Commit();
                    doc.Regenerate();

                    // Get all visible elements, excluding everything but the crop box
                    collector = new FilteredElementCollector(doc, view.Id);
                    collector.Excluding(shownElems);

                    viewCrop.cropElement = collector.FirstElement();
                    viewCrop.cropRegion = view.GetCropRegionShapeManager().GetCropShape().First();

                }
                tGroup.RollBack();
            }

            TransactionManager.Instance.TransactionTaskDone();

            return viewCrop;

        }

        #endregion

    }
}
