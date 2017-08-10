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

namespace Parameters
{
    /// <summary>
    /// Methods for managing parameters
    /// </summary>
    public class FamilyParameter
    {
        internal FamilyParameter() { }

        /// <summary>
        /// Creates a new family parameter for the currently open family document
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="group">The group to place the parameter in</param>
        /// <param name="type">The type of parameter</param>
        /// <param name="instance">Create as an instance parameter</param>
        /// <returns>The name of the created parameter</returns>
        public static string CreateFamilyParameter(string name, string group, string type, bool instance)
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            var parameterType = Autodesk.Revit.DB.ParameterType.Text;
            if (!System.Enum.TryParse<Autodesk.Revit.DB.ParameterType>(type, out parameterType))
                throw new System.Exception("Parameter type not found");


            var parameterGroup = Autodesk.Revit.DB.BuiltInParameterGroup.PG_TEXT;
            if (!System.Enum.TryParse<Autodesk.Revit.DB.BuiltInParameterGroup>(group, out parameterGroup))
                throw new System.Exception("Parameter group not found");


            Autodesk.Revit.DB.FamilyParameter returnVal;

            // Do the actual transaction
            TransactionManager.Instance.EnsureInTransaction(doc);
            returnVal = doc.FamilyManager.AddParameter(name, parameterGroup, parameterType, instance);
            TransactionManager.Instance.TransactionTaskDone();

            return returnVal.Definition.Name;

        }
    }
}
