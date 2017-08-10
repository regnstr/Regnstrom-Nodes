using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Revit.Elements;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

using Autodesk.DesignScript.Runtime;

using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Elements
{
    /// <summary>
    /// Utility methods for FamilyInstance
    /// </summary>
    public class FamilyInstance
    {
        internal FamilyInstance() { }

        /// <summary>
        /// Returns true if the familyInstance has been flipped
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public static bool Flipped(Revit.Elements.FamilyInstance familyInstance)
        {
            Autodesk.Revit.DB.FamilyInstance uwFi = familyInstance.InternalElement as Autodesk.Revit.DB.FamilyInstance;

            if(!uwFi.CanFlipFacing && !uwFi.CanFlipHand)
            {
                throw new ArgumentException("familyInstance cannot be flipped");
            }

            bool flipped = (uwFi.FacingFlipped != uwFi.HandFlipped);
            return flipped;
        }
    }

    
}

/*
namespace Parameters
{
    public class TestingParameters
    {
        internal TestingParameters() { }

        [MultiReturn(new[] { "par1", "par2" })]
        public static Dictionary<string, dynamic> TestParameters()
        {
            Document doc = DocumentManager.Instance.CurrentDBDocument;
            Application app = doc.Application;
            DefinitionFile defFile = app.OpenSharedParameterFile();

            StringBuilder fileInformation = new StringBuilder(500);

            // get the file name 
            fileInformation.AppendLine("File Name: " + defFile.Filename);

            // iterate the Definition groups of this file
            foreach (DefinitionGroup myGroup in defFile.Groups)
            {
                // get the group name
                fileInformation.AppendLine("Group Name: " + myGroup.Name);

                // iterate the difinitions
                foreach (Definition definition in myGroup.Definitions)
                {
                    // get definition name
                    fileInformation.AppendLine("Definition Name: " + definition.Name);
                }
            }
            return new Dictionary<string, dynamic>
            {
                { "par1", fileInformation },
                { "par2", fileInformation }
            };
        }
    }
}
*/