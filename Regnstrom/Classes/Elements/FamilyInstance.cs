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

        /// <summary>
        /// Returns the value of the FromRoom and ToRoom properties of the door or window. Returns null
        /// if the familyInstance is not a door or window, or if there simply is
        /// no value (eg. an exterior door/window)
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>The rooms on either side of this familyInstance</returns>
        ///
        [MultiReturn(new[] { "FromRoom", "ToRoom" })]
        public static Dictionary<string, Room> FromAndToRooms(Revit.Elements.FamilyInstance familyInstance, Revit.Elements.Element phase)
        {
            Autodesk.Revit.DB.FamilyInstance uwFi = familyInstance.InternalElement as Autodesk.Revit.DB.FamilyInstance;
            Autodesk.Revit.DB.Phase uwPhase = phase.InternalElement as Phase;

            Room fromRoom = null;
            Room toRoom = null;

            var tempFromRoom = uwFi.get_FromRoom(uwPhase);
            var tempToRoom = uwFi.get_ToRoom(uwPhase);

            if (tempFromRoom != null)
            {
                fromRoom = tempFromRoom.ToDSType(true) as Room;
            }

            if (tempToRoom != null)
            {
                toRoom = tempToRoom.ToDSType(true) as Room;
            }


            var returnDict = new Dictionary<string, Room>
            {
                {"FromRoom", fromRoom},
                {"ToRoom", toRoom},
            };

            return returnDict;
        }

        /// <summary>
        /// Flips the direction of FromRoom/ToRoom for this door or window.
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public static Revit.Elements.FamilyInstance FlipFromToRoom(Revit.Elements.FamilyInstance familyInstance)
        {
            Autodesk.Revit.DB.FamilyInstance uwFi = familyInstance.InternalElement as Autodesk.Revit.DB.FamilyInstance;

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            uwFi.FlipFromToRoom();

            TransactionManager.Instance.TransactionTaskDone();
            //DocumentManager.Instance.CurrentDBDocument.Regenerate();

            return familyInstance;
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