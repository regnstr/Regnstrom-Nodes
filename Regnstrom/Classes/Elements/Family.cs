using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Revit.Elements;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

using Autodesk.DesignScript.Runtime;

using System.IO;

using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Elements
{
    /// <summary>
    /// Utility methods for Family
    /// </summary>
    public class Family
    {
        internal Family() { }

        /// <summary>
        /// Saves a .rfa file to the specified path.
        /// </summary>
        /// <param name="family">The loaded family to be saved</param>
        /// <param name="path">Target file path</param>
        /// <param name="fileName">Target file fame (without .rfa)</param>
        public static bool SaveAs(Revit.Elements.Family family, string path, string fileName)
        {
            var intFam = family.InternalElement as Autodesk.Revit.DB.Family;

            TransactionManager.Instance.ForceCloseTransaction();
            
            var famDoc = DocumentManager.Instance.CurrentDBDocument.EditFamily(intFam);
            famDoc.SaveAs(Path.Combine(path, fileName + ".rfa"));
            famDoc.Close(false);

            return true;
        }
    }
}
