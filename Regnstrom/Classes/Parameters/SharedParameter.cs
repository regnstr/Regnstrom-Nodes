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
    /// Shared parameters
    /// </summary>
    public class SharedParameter
    { 
        internal SharedParameter() { }

        /// <summary>
        /// Displays information about the current shared parameter file.
        /// </summary>
        /// <returns></returns>
        [MultiReturn(new[] { "fileName", "groupNames", "parameterNames" })]
        public static Dictionary<string, dynamic> FileInformation()
        {
            
            DefinitionFile df = DocumentManager.Instance.CurrentDBDocument.Application.OpenSharedParameterFile();

            List<string> groupNames = new List<string>();
            List<List<string>> parameterNames = new List<List<string>>();
            //List<List<string[]>> parameterInfo = new List<List<string[]>>();

            foreach (DefinitionGroup dg in df.Groups)
            {
                groupNames.Add(dg.Name);

                // Temporary list used to iterate all parameters in a group
                //List<string[]> tempParameterInfo = new List<string[]>();
                List<string> tempParameterNames = new List<string>();

                // Iterate all definitions and save their information
                foreach (Definition def in dg.Definitions)
                {
                    /*tempParameterInfo.Add(new string[]{
                        def.Name,
                        def.ParameterType.ToString(),
                        def.UnitType.ToString()
                    });*/
                    tempParameterNames.Add(def.Name);
                }

                // Add the temporary list to the returned list
                //parameterInfo.Add(tempParameterInfo);
                parameterNames.Add(tempParameterNames);

            }

            return new Dictionary<string, dynamic>() {
                { "fileName", df.Filename },
                { "groupNames", groupNames },
                { "parameterNames", parameterNames }
            };

        }
    }
}
