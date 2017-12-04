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

namespace Regnstrom.Views
{
    /// <summary>
    /// Extensions for handling views.
    /// </summary>
    public class Section
    {
        internal Section() { }

        /// <summary>
        /// Offsets a section view in its view direction
        /// </summary>
        /// <param name="section">The view to move</param>
        /// <param name="distance">Distance to move (IN UNITS!?)</param>
        /// <returns></returns>
        public static Revit.Elements.Views.SectionView Offset(Revit.Elements.Views.SectionView section, float distance)
        {

            // Prelim stuff
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var iView = section.InternalElement as Autodesk.Revit.DB.ViewSection;
            var sectionElement = Regnstrom.Views.View.GetCropElement(section).InternalElement;

            // Find dirction to move view by negating its ViewDirection vector (it points towards the screen)
            var direction = iView.ViewDirection.Negate();

            // Multiply vector to get the right distance
            var movement = direction.Multiply(UnitUtils.ConvertToInternalUnits(distance, DisplayUnitType.DUT_MILLIMETERS));

            // Move view
            TransactionManager.Instance.EnsureInTransaction(doc);
            ElementTransformUtils.MoveElement(doc, sectionElement.Id, movement);
            TransactionManager.Instance.TransactionTaskDone();

            // Return the view
            return iView.ToDSType(true) as Revit.Elements.Views.SectionView;

        }

        /// <summary>
        /// Flips a section
        /// </summary>
        /// <param name="section">The section to flip</param>
        /// <param name="copy">As a copy</param>
        /// <returns></returns>
        public static Revit.Elements.Views.SectionView Flip(Revit.Elements.Views.SectionView section, bool copy = false)
        {
            // Prelim stuff
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var iView = section.InternalElement as Autodesk.Revit.DB.ViewSection;
            var sectionElement = Regnstrom.Views.View.GetCropElement(section).InternalElement;

            // Construct flip plane
            var flipPlane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(iView.ViewDirection, iView.Origin);

            // Flip view
            TransactionManager.Instance.EnsureInTransaction(doc);
            // (yes, MirrorElements, because MirrorElement forces you to copy the element)
            ElementTransformUtils.MirrorElements(doc, new[] { sectionElement.Id }, flipPlane, copy);
            TransactionManager.Instance.TransactionTaskDone();

            // Return the view
            return iView.ToDSType(true) as Revit.Elements.Views.SectionView;


        }

    }
}
