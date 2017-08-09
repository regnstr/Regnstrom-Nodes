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
    public class ParameterBindings
    {
        internal ParameterBindings() { }

        public static dynamic BindingMap()
        {

            List<dynamic> output = new List<dynamic>();

            BindingMap bm = DocumentManager.Instance.CurrentDBDocument.ParameterBindings;

            foreach (dynamic dyn in bm)
            {
                output.Add(dyn);
            }

            return output;

        }
    }
}
