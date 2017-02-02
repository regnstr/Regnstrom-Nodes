using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Revit.Elements;
using Autodesk.Revit.DB;

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
