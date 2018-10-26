using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// RevitAPI
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

// Dynamo Core
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

// Dynamo Revit
using Revit.Elements;
using Revit.GeometryConversion;

using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Regnstrom.Elements
{
    public static class Room
    {
        /// <summary>
        /// Create an unplaced room in the specified phase.
        /// </summary>
        /// <param name="phase">The phase when the new room exists</param>
        /// <returns>The new Room</returns>
        public static Revit.Elements.Room ByPhase(Revit.Elements.Element phase)
        {
            var dDoc = DocumentManager.Instance.CurrentDBDocument;
            var cDoc = dDoc.Create;
            
            Phase iPhase = phase.InternalElement as Phase;
            if(iPhase != null)
            {
                TransactionManager.Instance.EnsureInTransaction(dDoc);

                Revit.Elements.Room newRoom = cDoc.NewRoom(iPhase).ToDSType(true) as Revit.Elements.Room;

                TransactionManager.Instance.TransactionTaskDone();

                dDoc.Regenerate();

                return newRoom;

            } else
            {
                throw new ArgumentException("Could not cast the provided Element to a Revit phase.");
            }
            
        }

        /// <summary>
        /// Projects the borders of one or more rooms to a new view (level) and creates Room Separation Lines at that level.
        /// </summary>
        /// <param name="room">The rooms whose borders will be copied.</param>
        /// <param name="view">The target view.</param>
        /// <returns></returns>
        [MultiReturn(new[] {"Room", "View", "Curves"})]
        public static Dictionary<string, dynamic> CopyBordersToView(Revit.Elements.Room room, Revit.Elements.Views.FloorPlanView view)
        {
            var dDoc = DocumentManager.Instance.CurrentDBDocument;
            var cDoc = dDoc.Create;

            // Construct the return dictionary
            var returnDict = new Dictionary<string, dynamic>()
            {
                {"Room", room },
                {"View", view },
                {"Curves", null }
            };


            // Extract the Revit objects from the view and room
            var iView = view.InternalElement as Autodesk.Revit.DB.ViewPlan;
            var iRoom = room.InternalElement as Autodesk.Revit.DB.Architecture.Room;

            // Retrieve the target level for the rooms
            var targetLevel = iView.GenLevel;
            var baseLevel = iRoom.Level;

            // Calculate how much the curves should be moved
            double distanceToMove = targetLevel.Elevation - baseLevel.Elevation;

            // Convert to a real unit
            double distanceInScience = UnitUtils.ConvertFromInternalUnits(distanceToMove, DisplayUnitType.DUT_MILLIMETERS);

            // Create an origin for the plane on which to project the room borders, and create the plane
            var newOrigin = Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, distanceInScience);
            var projPlane = Autodesk.DesignScript.Geometry.Plane.ByOriginNormal(newOrigin, Vector.ZAxis());

            // Extract room borders using the Revit Nodes method
            var borders = room.CenterBoundary;
            var projectedBorders = new List<List<Autodesk.DesignScript.Geometry.Curve>>();
            
            List<CurveArray> lca = new List<CurveArray>();

            // Extract revit curves and project them to the new level (it works without the projection but produces strange behaviour)
            foreach (IEnumerable<Autodesk.DesignScript.Geometry.Curve> enumerable in borders)
            {

                var newList = new List<Autodesk.DesignScript.Geometry.Curve>();
                projectedBorders.Add(newList);

                CurveArray ca = new CurveArray();
                lca.Add(ca);

                foreach (Autodesk.DesignScript.Geometry.Curve curve in enumerable)
                {
                    var movedCurve = curve.Translate(Vector.ZAxis(), distanceInScience) as Autodesk.DesignScript.Geometry.Curve;

                    //var projectedCurves = curve.Project(projPlane, Vector.ZAxis())[0] as Autodesk.DesignScript.Geometry.Curve;
                    
                    newList.Add(movedCurve);
                    ca.Append(movedCurve.ToRevitType());
                }
            }

            // Add the curves to the output dict
            returnDict["Curves"] = projectedBorders;

            // Extract the target SketchPlane
            var mySketch = (view.InternalElement as ViewPlan).SketchPlane;

            // Create the new room boundaries
            TransactionManager.Instance.EnsureInTransaction(dDoc);

            foreach (CurveArray ca in lca)
            {
                cDoc.NewRoomBoundaryLines(mySketch, ca, view.InternalElement as View);
            }

            TransactionManager.Instance.TransactionTaskDone();
            

            return returnDict;
        }

        /// <summary>
        /// Moves the rooms from one level to another. NOTE: This node produces an error about the room having a negative height, but by pressing "Adjust Limits..." the rooms will be moved correctly.
        /// </summary>
        /// <param name="room">The room to move.</param>
        /// <param name="view">The target view (level) to move to.</param>
        /// <returns></returns>
        public static Revit.Elements.Room ChangeLevel(Revit.Elements.Room room, Revit.Elements.Views.FloorPlanView view)
        {
            var dDoc = DocumentManager.Instance.CurrentDBDocument;
            var cDoc = dDoc.Create;

            // Extract the Revit objects from the view and room
            var iView = view.InternalElement as Autodesk.Revit.DB.ViewPlan;
            var iRoom = room.InternalElement as Autodesk.Revit.DB.Architecture.Room;

            // Retrieve the target level for the rooms
            var targetLevel = iView.GenLevel;
            var baseLevel = iRoom.Level;

            
            // For testing purposes
            var returnList = new List<dynamic>();

            // Get the target topology and plan circuits
            var topology = dDoc.get_PlanTopology(targetLevel);
            var circuits = topology.Circuits;


            /* Match the old room position with the new one
             */

            bool targetFound = false;
            PlanCircuit targetPC = null;

            foreach (PlanCircuit pc in circuits)
            {
                Autodesk.Revit.DB.UV uv = pc.GetPointInside();
                Autodesk.Revit.DB.XYZ xyz = new XYZ(uv.U, uv.V, baseLevel.Elevation);
                targetFound = iRoom.IsPointInRoom(xyz);

                if (targetFound)
                {
                    targetPC = pc;
                    break;
                }
            }

            if (!targetFound)
            {
                return null;
            }


            /* Unplace the old room
             */

            TransactionManager.Instance.EnsureInTransaction(dDoc);

            iRoom.Unplace();

            TransactionManager.Instance.TransactionTaskDone();
            

            /* Place the new room
             */
            
            TransactionManager.Instance.EnsureInTransaction(dDoc);

            var newRoom = cDoc.NewRoom(iRoom, targetPC);

            TransactionManager.Instance.TransactionTaskDone();

            //dDoc.Regenerate();
            
            return newRoom.ToDSType(true) as Revit.Elements.Room;
        }

        /// <summary>
        /// Utility node to double check how many plan circuits exist, and where they are located.
        /// </summary>
        /// <param name="view">The view to check.</param>
        /// <returns></returns>
        public static List<Autodesk.DesignScript.Geometry.Point> GetCircuitPoints(Revit.Elements.Views.FloorPlanView view)
        {
            var dDoc = DocumentManager.Instance.CurrentDBDocument;
            var cDoc = dDoc.Create;

            // Extract the Revit objects from the view and room
            var iView = view.InternalElement as Autodesk.Revit.DB.ViewPlan;

            // Get the target topology and plan circuits
            var topology = dDoc.get_PlanTopology(iView.GenLevel);
            var circuits = topology.Circuits;

            var points = new List<Autodesk.DesignScript.Geometry.Point>();
            var borders = new List<Autodesk.DesignScript.Geometry.Curve>();

            foreach (PlanCircuit pc in circuits)
            {

                Autodesk.Revit.DB.UV uv = pc.GetPointInside();
                var newU = UnitUtils.ConvertFromInternalUnits(uv.U, DisplayUnitType.DUT_MILLIMETERS);
                var newV = UnitUtils.ConvertFromInternalUnits(uv.V, DisplayUnitType.DUT_MILLIMETERS);
                //Autodesk.Revit.DB.XYZ xyz = new XYZ(uv.U, uv.V, baseLevel.Elevation);

                points.Add(Autodesk.DesignScript.Geometry.Point.ByCoordinates(newU, newV));
            }

            return points;
        }
    }
}
