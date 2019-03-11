using System;
using KSPCDriver;
using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;
using System.Collections.Generic;

namespace KSPCDriver.KSPBridge
{
    public class KSPVesselBridge
    {
        public static VesselData GetVesselData(Vessel vessel)
        {
            VesselData _data = new VesselData();
            //tmp info
            List<Part> engines = Utils.GetListOfActivatedEngines(vessel);
            //
            _data.AP = (float)vessel.orbit.ApA;
            _data.PE = (float)vessel.orbit.PeA;
            _data.SemiMajorAxis = (float)vessel.orbit.semiMajorAxis;
            _data.SemiMinorAxis = (float)vessel.orbit.semiMinorAxis;
            _data.e = (float)vessel.orbit.eccentricity;
            _data.inc = (float)vessel.orbit.inclination;
            _data.VVI = (float)vessel.verticalSpeed;
            _data.G = (float)vessel.geeForce;
            _data.TAp = (int)Math.Round(vessel.orbit.timeToAp);
            _data.TPe = (int)Math.Round(vessel.orbit.timeToPe);
            _data.Density = (float)vessel.atmDensity;
            _data.TrueAnomaly = (float)vessel.orbit.trueAnomaly;
            _data.period = (int)Math.Round(vessel.orbit.period);
            //
            double ASL = vessel.mainBody.GetAltitude(vessel.CoM);
            double AGL = (ASL - vessel.terrainAltitude);
            if (AGL < ASL)
                _data.RAlt = (float)AGL;
            else
                _data.RAlt = (float)ASL;
            //
            _data.Alt = (float)ASL;
            _data.Vsurf = (float)vessel.srfSpeed;
            _data.Lat = (float)vessel.latitude;
            _data.Lon = (float)vessel.longitude;

            IOResource tmp = Utils.GetResourceTotal(vessel, "LiquidFuel");
            _data.LiquidFuelTot = tmp.Max;
            _data.LiquidFuel = tmp.Current;

            _data.LiquidFuelTotS = (float)Utils.ProspectForResourceMax("LiquidFuel", engines);
            _data.LiquidFuelS = (float)Utils.ProspectForResource("LiquidFuel", engines);

            tmp = Utils.GetResourceTotal(vessel, "Oxidizer");
            _data.OxidizerTot = tmp.Max;
            _data.Oxidizer = tmp.Current;

            _data.OxidizerTotS = (float)Utils.ProspectForResourceMax("Oxidizer", engines);
            _data.OxidizerS = (float)Utils.ProspectForResource("Oxidizer", engines);

            tmp = Utils.GetResourceTotal(vessel, "ElectricCharge");
            _data.EChargeTot = tmp.Max;
            _data.ECharge = tmp.Current;
            tmp = Utils.GetResourceTotal(vessel, "MonoPropellant");
            _data.MonoPropTot = tmp.Max;
            _data.MonoProp = tmp.Current;
            tmp = Utils.GetResourceTotal(vessel, "IntakeAir");
            _data.IntakeAirTot = tmp.Max;
            _data.IntakeAir = tmp.Current;
            tmp = Utils.GetResourceTotal(vessel, "SolidFuel");
            _data.SolidFuelTot = tmp.Max;
            _data.SolidFuel = tmp.Current;
            tmp = Utils.GetResourceTotal(vessel, "XenonGas");
            _data.XenonGasTot = tmp.Max;
            _data.XenonGas = tmp.Current;
            //
            double missionTime = vessel.missionTime;
            _data.MissionTime = (UInt32)Math.Round(missionTime);
            _data.VOrbit = (float)vessel.orbit.GetVel().magnitude;
            //
            _data.MNTime = 0;
            _data.MNDeltaV = 0;
            if (vessel.patchedConicSolver != null && vessel.patchedConicSolver.maneuverNodes != null && vessel.patchedConicSolver.maneuverNodes.Count > 0)
            {
                _data.MNTime = (UInt32)Math.Round(vessel.patchedConicSolver.maneuverNodes[0].UT - Planetarium.GetUniversalTime());
                _data.MNDeltaV = (float)vessel.patchedConicSolver.maneuverNodes[0].GetBurnVector(vessel.patchedConicSolver.maneuverNodes[0].patch).magnitude; //Added JS
            }
            //
            Quaternion attitude = Utils.updateHeadingPitchRollField(vessel);

            _data.Roll = (float)((attitude.eulerAngles.z > 180) ? (attitude.eulerAngles.z - 360.0) : attitude.eulerAngles.z);
            _data.Pitch = (float)((attitude.eulerAngles.x > 180) ? (360.0 - attitude.eulerAngles.x) : -attitude.eulerAngles.x);
            _data.Heading = (float)attitude.eulerAngles.y;

            //_data.ControlStatus((int)enumAG.SAS, vessel.ActionGroups[KSPActionGroup.SAS]);
            //_data.ControlStatus((int)enumAG.RCS, vessel.ActionGroups[KSPActionGroup.RCS]);
            //_data.ControlStatus((int)enumAG.Light, vessel.ActionGroups[KSPActionGroup.Light]);
            //_data.ControlStatus((int)enumAG.Gear, vessel.ActionGroups[KSPActionGroup.Gear]);
            //_data.ControlStatus((int)enumAG.Brakes, vessel.ActionGroups[KSPActionGroup.Brakes]);
            //_data.ControlStatus((int)enumAG.Abort, vessel.ActionGroups[KSPActionGroup.Abort]);
            //_data.ControlStatus((int)enumAG.Custom01, vessel.ActionGroups[KSPActionGroup.Custom01]);
            //_data.ControlStatus((int)enumAG.Custom02, vessel.ActionGroups[KSPActionGroup.Custom02]);
            //_data.ControlStatus((int)enumAG.Custom03, vessel.ActionGroups[KSPActionGroup.Custom03]);
            //_data.ControlStatus((int)enumAG.Custom04, vessel.ActionGroups[KSPActionGroup.Custom04]);
            //_data.ControlStatus((int)enumAG.Custom05, vessel.ActionGroups[KSPActionGroup.Custom05]);
            //_data.ControlStatus((int)enumAG.Custom06, vessel.ActionGroups[KSPActionGroup.Custom06]);
            //_data.ControlStatus((int)enumAG.Custom07, vessel.ActionGroups[KSPActionGroup.Custom07]);
            //_data.ControlStatus((int)enumAG.Custom08, vessel.ActionGroups[KSPActionGroup.Custom08]);
            //_data.ControlStatus((int)enumAG.Custom09, vessel.ActionGroups[KSPActionGroup.Custom09]);
            //_data.ControlStatus((int)enumAG.Custom10, vessel.ActionGroups[KSPActionGroup.Custom10]);

            if (vessel.orbit.referenceBody != null)
            {
                _data.SOINumber = Utils.GetSOINumber(vessel.orbit.referenceBody.name);
            }

            _data.MaxOverHeat = Utils.GetMaxOverHeat(vessel);
            _data.MachNumber = (float)vessel.mach;
            _data.IAS = (float)vessel.indicatedAirSpeed;

            _data.CurrentStage = (byte)StageManager.CurrentStage;
            _data.TotalStage = (byte)StageManager.StageCount;

            //target distance and velocity stuff                    

            _data.TargetDist = 0;
            _data.TargetV = 0;

            if (KSPVesselBridge._anyTarget())
            {
                _data.TargetDist = (float)Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetVessel().transform.position, vessel.transform.position);
                _data.TargetV = (float)FlightGlobals.ship_tgtVelocity.magnitude;
            }


            _data.NavballSASMode = (byte)(((int)FlightGlobals.speedDisplayMode + 1) << 4); //get navball speed display mode
            if (vessel.ActionGroups[KSPActionGroup.SAS])
            {
                _data.NavballSASMode = (byte)(((int)FlightGlobals.ActiveVessel.Autopilot.Mode + 1) | _data.NavballSASMode);
            }

            return _data;
        }
        public static void SetControllerOnVessel(VesselControls controls, VesselControls previousControls, Vessel vessel)
        {
            _setValueIfDiffer(controls.RCS, previousControls.RCS, vessel, KSPActionGroup.RCS);
            _setValueIfDiffer(controls.SAS, previousControls.SAS, vessel, KSPActionGroup.SAS);
            _setValueIfDiffer(controls.Light, previousControls.Light, vessel, KSPActionGroup.Light);
            _setValueIfDiffer(controls.Brakes, previousControls.Brakes, vessel, KSPActionGroup.Brakes);
            _setValueIfDiffer(controls.Abort, previousControls.Abort, vessel, KSPActionGroup.Abort);
            _setValueIfDiffer(controls.Stage, previousControls.Stage, vessel, KSPActionGroup.Stage);
            _setSASModeIfDiffer(controls.SASMode, previousControls.SASMode, vessel);
            _setSpeedDisplayModeIfDiffer(controls.SpeedMode, previousControls.SpeedMode, vessel);
            _disableSASIfNeeded(controls, vessel);
            //control groups
            for (int x = 1; x < Definitions.CONTROL_GROUP_COUNT; x++)
            {
                //KSPActionGroup.Custom01 - 128 KSPActionGroup.Custom02 - 256...
                _setValueIfDiffer(controls.ControlGroup[x], previousControls.ControlGroup[x], vessel, (KSPActionGroup)(128 * x));
            }
        }
        public static void FillVesselStateWithController(FlightCtrlState vesselState, VesselControls control)
        {
            if (KSPCSettings.ThrottleEnable) vesselState.mainThrottle = control.Throttle;
            if (KSPCSettings.PitchEnable) vesselState.pitch = control.Pitch;
            if (KSPCSettings.RollEnable) vesselState.roll = control.Roll;
            if (KSPCSettings.YawEnable) vesselState.yaw = control.Yaw;
            if (KSPCSettings.TXEnable) vesselState.X = control.TX;
            if (KSPCSettings.TYEnable) vesselState.Y = control.TY;
            if (KSPCSettings.TZEnable) vesselState.Z = control.TZ;
            if (KSPCSettings.WheelSteerEnable) vesselState.wheelSteer = control.WheelSteer;
            if (KSPCSettings.WheelThrottleEnable) vesselState.wheelThrottle = control.WheelThrottle;
        }

        //
        private static void _setValueIfDiffer(bool curr, bool last, Vessel vessel, KSPActionGroup action)
        {
            if (curr != last)
            {
                vessel.ActionGroups.SetGroup(action, curr);
                Utils.PrintDebugMessage("Setting " + action.ToString() + " with value: " + curr.ToString());
                //specific actions
                if (KSPActionGroup.Stage == action && curr)
                {
                    StageManager.ActivateNextStage();
                }
            }
        }
        private static void _setSASModeIfDiffer(int curr, int last, Vessel vessel)
        {
            if (curr != last && curr != 0 && curr < Definitions.CONTROL_GROUP_COUNT)
            {
                //check if can set mode
                if (vessel.Autopilot.CanSetMode((VesselAutopilot.AutopilotMode)(curr - 1)))
                {
                    vessel.Autopilot.SetMode((VesselAutopilot.AutopilotMode)curr - 1);
                }
                Utils.PrintDebugMessage("Setting SASMODE with value: " + curr.ToString());
            }
        }
        private static void _setSpeedDisplayModeIfDiffer(int curr, int last, Vessel vessel)
        {
            if (curr != last && curr != 0 && (curr != 3 && _anyTarget()))
            {
                FlightGlobals.SetSpeedMode((FlightGlobals.SpeedDisplayModes)(curr - 1));
                Utils.PrintDebugMessage("Setting SpeedMode with value: " + curr.ToString());
            }
        }
        private static void _disableSASIfNeeded(VesselControls controls, Vessel vessel)
        {
            // temporarily disengage SAS while steering
            if (Math.Abs(controls.Pitch) > KSPCSettings.SASTol || Math.Abs(controls.Roll) > KSPCSettings.SASTol ||
                Math.Abs(controls.Yaw) > KSPCSettings.SASTol)
            {
                vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
            }
            else if (controls.SAS == true)
            {
                vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            }
        }
        //private utils
        private static bool _anyTarget()
        {
            return ((FlightGlobals.fetch.VesselTarget != null) && (FlightGlobals.fetch.VesselTarget.GetVessel() != null));
        }
    }
}
