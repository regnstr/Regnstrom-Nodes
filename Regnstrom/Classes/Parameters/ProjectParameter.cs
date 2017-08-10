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
    /// Project parameters.
    /// </summary>
    public class ProjectParameter
    {
        internal ProjectParameter() { }

        /// <summary>
        /// Creates a project parameter based on the definition of a shared parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="instance">Set to true to create an instance parameter, false to create a type parameter.</param>
        /// <param name="categories">The categores to add this parameter to, by their built in parameter name.</param>
        /// <param name="parameterGroup">The parameter group for this parameter.</param>
        /// <returns></returns>
        public static bool BySharedParameter(string parameterName, bool instance, string[] categories, string parameterGroup = "PG_TEXT")
        {
            // Get current document
            Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Get the BindingMap (contains all bindings)
            BindingMap bm = doc.ParameterBindings;
            DefinitionFile df = doc.Application.OpenSharedParameterFile();

            /*
             * CATEGORES
             */
            // Create a CategorySet
            CategorySet myCategories = doc.Application.Create.NewCategorySet();

            foreach (string s in categories)
            {
                // Try to find the BuiltInCategory
                Autodesk.Revit.DB.BuiltInCategory tempCategory;
                if (!Enum.TryParse<Autodesk.Revit.DB.BuiltInCategory>(s, out tempCategory))
                    throw new Exception(string.Format("BuiltInCategory {0} not found.", s));

                // Add it to the CategorySet
                myCategories.Insert(doc.Settings.Categories.get_Item(tempCategory));
            }

            /*
             * PARAMETER GROUP
             */
            BuiltInParameterGroup pGroup;
            if (!System.Enum.TryParse<Autodesk.Revit.DB.BuiltInParameterGroup>(parameterGroup, out pGroup))
                throw new Exception(string.Format("BuiltInParameterGroup {0} not found.", parameterGroup));


            /*
             * CREATE THE BINDING
             */
            dynamic myBinding;

            // Create the InstanceBinding or the TypeBinding
            if (instance)
            {
                myBinding = doc.Application.Create.NewInstanceBinding(myCategories);
            }
            else
            {
                myBinding = doc.Application.Create.NewTypeBinding(myCategories);
            }

            // List to record outputs
            bool bindOk;

            // Ensure in transaction
            TransactionManager.Instance.EnsureInTransaction(doc);

            Definition def = FindDefinitionByName(parameterName, df);
            if (def == null)
            {
                throw new Exception(string.Format("Parameter definition for \"{0}\" not found.", parameterName));
            } else
            {
                bindOk = bm.Insert(def, myBinding, pGroup);
            }

            // Transaction done
            TransactionManager.Instance.TransactionTaskDone();

            return bindOk;
        }

        private static Definition FindDefinitionByName(string name, DefinitionFile df)
        {
            foreach (DefinitionGroup dg in df.Groups)
            {
                foreach (Definition d in dg.Definitions)
                {
                    if (d.Name.Equals(name))
                    {
                        return d;
                    }
                }
            }
            return null;
        }
    }
}
