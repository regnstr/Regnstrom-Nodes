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
    public class SharedParameter
    { 
        internal SharedParameter() { }

        [MultiReturn(new[] { "fileName", "groupNames", "parameterInfo" })]
        public static Dictionary<string, dynamic> SharedParameterFilename()
        {
            
            DefinitionFile df = DocumentManager.Instance.CurrentDBDocument.Application.OpenSharedParameterFile();

            List<string> groupNames = new List<string>();
            List<List<string[]>> parameterInfo = new List<List<string[]>>();

            foreach (DefinitionGroup dg in df.Groups)
            {
                groupNames.Add(dg.Name);

                // Temporary list used to iterate all parameters in a group
                List<string[]> tempParameterInfo = new List<string[]>();

                // Iterate all definitions
                foreach (Definition def in dg.Definitions)
                {
                    tempParameterInfo.Add(new string[]{
                        def.Name,
                        def.ParameterType.ToString(),
                        def.UnitType.ToString()
                    });
                }

                // Add the temporary list to the returned list
                parameterInfo.Add(tempParameterInfo);
                    
            }

            return new Dictionary<string, dynamic>() {
                { "fileName", df.Filename },
                { "groupNames", groupNames },
                { "parameterInfo", parameterInfo }
            };

        }

        public static List<bool> CreateInstanceParameter()
        {

            // Get current document
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Get the BindingMap (contains all bindings)
            BindingMap bm = doc.ParameterBindings;
            DefinitionFile df = doc.Application.OpenSharedParameterFile();
            
            // Create a CategorySet
            CategorySet myCategories = doc.Application.Create.NewCategorySet();
            Autodesk.Revit.DB.Category myCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
            myCategories.Insert(myCategory);

            // Create the InstanceBinding
            InstanceBinding ib = doc.Application.Create.NewInstanceBinding(myCategories);
            
            // List to record outputs
            List<bool> instanceBindOk = new List<bool>();

            // Ensure in transaction
            TransactionManager.Instance.EnsureInTransaction(doc);

            foreach (DefinitionGroup dg in df.Groups)
            {
                foreach (Definition d in dg.Definitions)
                {
                    instanceBindOk.Add(bm.Insert(d, ib, BuiltInParameterGroup.PG_TEXT));
                }
            }
            
            // Transaction done
            TransactionManager.Instance.TransactionTaskDone();

            return instanceBindOk;
            

        }

    }
}
