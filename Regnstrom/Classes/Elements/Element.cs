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

namespace Regnstrom.Elements
{
    public static class Element
    {
        public static Revit.Elements.Element Pin(this Revit.Elements.Element element)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            element.InternalElement.Pinned = true;
            DocumentManager.Regenerate();
            TransactionManager.Instance.TransactionTaskDone();
            return element;
        }

        public static Revit.Elements.Element Unpin(this Revit.Elements.Element element)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            element.InternalElement.Pinned = false;
            DocumentManager.Regenerate();
            TransactionManager.Instance.TransactionTaskDone();
            return element;
        }
    }
}
