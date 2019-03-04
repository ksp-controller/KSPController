using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;

using Psimax.IO.Ports;
using KSP.IO;
using UnityEngine;
using KSP.UI.Screens;
//
using KSPCDriver.Enums;
using KSPCDriver.Serial;

namespace KSPCDriver
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KSPSerialIO : MonoBehaviour
    {
        private double lastUpdate = 0.0f;
        private double deltaT = 1.0f;
        private double missionTime = 0;
        private double missionTimeOld = 0;
        private double theTime = 0;

        public double refreshrate = 1.0f;
        public static Vessel ActiveVessel = new Vessel();

        public Guid VesselIDOld;

        IOResource TempR = new IOResource();
        private ScreenMessageStyle KSPIOScreenStyle = ScreenMessageStyle.UPPER_RIGHT;

        void Awake()
        {
            ScreenMessages.PostScreenMessage("IO awake", 10f, KSPIOScreenStyle);
            refreshrate = KSPCSettings.refreshrate;
        }

        void Start()
        {
            if (KSPCSerialPort.DisplayFound)
            {
                if (!KSPCSerialPort.Port.IsOpen)
                {
                    ScreenMessages.PostScreenMessage("Starting serial port " + KSPCSerialPort.Port.PortName, 10f, KSPIOScreenStyle);

                    try
                    {
                        KSPCSerialPort.Port.Open();
                        Thread.Sleep(KSPCSettings.HandshakeDelay);
                    }
                    catch (Exception e)
                    {
                        ScreenMessages.PostScreenMessage("Error opening serial port " + KSPCSerialPort.Port.PortName, 10f, KSPIOScreenStyle);
                        ScreenMessages.PostScreenMessage(e.Message, 10f, KSPIOScreenStyle);
                    }
                }
                else
                {
                    ScreenMessages.PostScreenMessage("Using serial port " + KSPCSerialPort.Port.PortName, 10f, KSPIOScreenStyle);

                    if (KSPCSettings.HandshakeDisable == 1)
                        ScreenMessages.PostScreenMessage("Handshake disabled");
                }

                Thread.Sleep(200);

                ActiveVessel.OnPostAutopilotUpdate -= AxisInput;
                ActiveVessel = FlightGlobals.ActiveVessel;
                ActiveVessel.OnPostAutopilotUpdate += AxisInput;

                //sync inputs at start
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, KSPCSerialPort.VControls.RCS);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, KSPCSerialPort.VControls.SAS);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, KSPCSerialPort.VControls.Lights);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, KSPCSerialPort.VControls.Gear);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, KSPCSerialPort.VControls.Brakes);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, KSPCSerialPort.VControls.Abort);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, KSPCSerialPort.VControls.Stage);

                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom01, KSPCSerialPort.VControls.ControlGroup[1]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom02, KSPCSerialPort.VControls.ControlGroup[2]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom03, KSPCSerialPort.VControls.ControlGroup[3]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom04, KSPCSerialPort.VControls.ControlGroup[4]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom05, KSPCSerialPort.VControls.ControlGroup[5]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom06, KSPCSerialPort.VControls.ControlGroup[6]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom07, KSPCSerialPort.VControls.ControlGroup[7]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom08, KSPCSerialPort.VControls.ControlGroup[8]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom09, KSPCSerialPort.VControls.ControlGroup[9]);
                ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom10, KSPCSerialPort.VControls.ControlGroup[10]);

            }
            else
            {
                ScreenMessages.PostScreenMessage("KerbalController not found", 10f, KSPIOScreenStyle);
            }
        }

        void Update()
        {
            if (FlightGlobals.ActiveVessel != null && KSPCSerialPort.Port.IsOpen)
            {
                //Debug.Log("KSPSerialIO: 1");
                //If the current active vessel is not what we were using, we need to remove controls from the old 
                //vessel and attache it to the current one
                if (ActiveVessel != null && ActiveVessel.id != FlightGlobals.ActiveVessel.id)
                {
                    ActiveVessel.OnPostAutopilotUpdate -= AxisInput;
                    ActiveVessel = FlightGlobals.ActiveVessel;
                    ActiveVessel.OnPostAutopilotUpdate += AxisInput;
                    //sync some inputs on vessel switch
                    ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, KSPCSerialPort.VControls.RCS);
                    ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, KSPCSerialPort.VControls.SAS);
                    Debug.Log("KSPSerialIO: ActiveVessel changed");
                }
                else
                {
                    ActiveVessel = FlightGlobals.ActiveVessel;
                }

                #region outputs
                theTime = Time.unscaledTime;
                if ((theTime - lastUpdate) > refreshrate)
                {
                    //Debug.Log("KSPSerialIO: 2");

                    lastUpdate = theTime;

                    List<Part> ActiveEngines = new List<Part>();
                    ActiveEngines = GetListOfActivatedEngines(ActiveVessel);

                    KSPCSerialPort.VData.AP = (float)ActiveVessel.orbit.ApA;
                    KSPCSerialPort.VData.PE = (float)ActiveVessel.orbit.PeA;
                    KSPCSerialPort.VData.SemiMajorAxis = (float)ActiveVessel.orbit.semiMajorAxis;
                    KSPCSerialPort.VData.SemiMinorAxis = (float)ActiveVessel.orbit.semiMinorAxis;
                    KSPCSerialPort.VData.e = (float)ActiveVessel.orbit.eccentricity;
                    KSPCSerialPort.VData.inc = (float)ActiveVessel.orbit.inclination;
                    KSPCSerialPort.VData.VVI = (float)ActiveVessel.verticalSpeed;
                    KSPCSerialPort.VData.G = (float)ActiveVessel.geeForce;
                    KSPCSerialPort.VData.TAp = (int)Math.Round(ActiveVessel.orbit.timeToAp);
                    KSPCSerialPort.VData.TPe = (int)Math.Round(ActiveVessel.orbit.timeToPe);
                    KSPCSerialPort.VData.Density = (float)ActiveVessel.atmDensity;
                    KSPCSerialPort.VData.TrueAnomaly = (float)ActiveVessel.orbit.trueAnomaly;
                    KSPCSerialPort.VData.period = (int)Math.Round(ActiveVessel.orbit.period);

                    //Debug.Log("KSPSerialIO: 3");
                    double ASL = ActiveVessel.mainBody.GetAltitude(ActiveVessel.CoM);
                    double AGL = (ASL - ActiveVessel.terrainAltitude);

                    if (AGL < ASL)
                        KSPCSerialPort.VData.RAlt = (float)AGL;
                    else
                        KSPCSerialPort.VData.RAlt = (float)ASL;

                    KSPCSerialPort.VData.Alt = (float)ASL;
                    KSPCSerialPort.VData.Vsurf = (float)ActiveVessel.srfSpeed;
                    KSPCSerialPort.VData.Lat = (float)ActiveVessel.latitude;
                    KSPCSerialPort.VData.Lon = (float)ActiveVessel.longitude;

                    TempR = GetResourceTotal(ActiveVessel, "LiquidFuel");
                    KSPCSerialPort.VData.LiquidFuelTot = TempR.Max;
                    KSPCSerialPort.VData.LiquidFuel = TempR.Current;

                    KSPCSerialPort.VData.LiquidFuelTotS = (float)ProspectForResourceMax("LiquidFuel", ActiveEngines);
                    KSPCSerialPort.VData.LiquidFuelS = (float)ProspectForResource("LiquidFuel", ActiveEngines);

                    TempR = GetResourceTotal(ActiveVessel, "Oxidizer");
                    KSPCSerialPort.VData.OxidizerTot = TempR.Max;
                    KSPCSerialPort.VData.Oxidizer = TempR.Current;

                    KSPCSerialPort.VData.OxidizerTotS = (float)ProspectForResourceMax("Oxidizer", ActiveEngines);
                    KSPCSerialPort.VData.OxidizerS = (float)ProspectForResource("Oxidizer", ActiveEngines);

                    TempR = GetResourceTotal(ActiveVessel, "ElectricCharge");
                    KSPCSerialPort.VData.EChargeTot = TempR.Max;
                    KSPCSerialPort.VData.ECharge = TempR.Current;
                    TempR = GetResourceTotal(ActiveVessel, "MonoPropellant");
                    KSPCSerialPort.VData.MonoPropTot = TempR.Max;
                    KSPCSerialPort.VData.MonoProp = TempR.Current;
                    TempR = GetResourceTotal(ActiveVessel, "IntakeAir");
                    KSPCSerialPort.VData.IntakeAirTot = TempR.Max;
                    KSPCSerialPort.VData.IntakeAir = TempR.Current;
                    TempR = GetResourceTotal(ActiveVessel, "SolidFuel");
                    KSPCSerialPort.VData.SolidFuelTot = TempR.Max;
                    KSPCSerialPort.VData.SolidFuel = TempR.Current;
                    TempR = GetResourceTotal(ActiveVessel, "XenonGas");
                    KSPCSerialPort.VData.XenonGasTot = TempR.Max;
                    KSPCSerialPort.VData.XenonGas = TempR.Current;

                    missionTime = ActiveVessel.missionTime;
                    deltaT = missionTime - missionTimeOld;
                    missionTimeOld = missionTime;

                    KSPCSerialPort.VData.MissionTime = (UInt32)Math.Round(missionTime);
                    KSPCSerialPort.VData.deltaTime = (float)deltaT;

                    KSPCSerialPort.VData.VOrbit = (float)ActiveVessel.orbit.GetVel().magnitude;

                    //Debug.Log("KSPSerialIO: 4");

                    KSPCSerialPort.VData.MNTime = 0;
                    KSPCSerialPort.VData.MNDeltaV = 0;

                    if (ActiveVessel.patchedConicSolver != null)
                    {
                        if (ActiveVessel.patchedConicSolver.maneuverNodes != null)
                        {
                            if (ActiveVessel.patchedConicSolver.maneuverNodes.Count > 0)
                            {
                                KSPCSerialPort.VData.MNTime = (UInt32)Math.Round(ActiveVessel.patchedConicSolver.maneuverNodes[0].UT - Planetarium.GetUniversalTime());
                                //ScreenMessages.PostScreenMessage(KSPCSerialPort.VData.MNTime.ToString());
                                //KSPCSerialPort.VData.MNDeltaV = (float)ActiveVessel.patchedConicSolver.maneuverNodes[0].DeltaV.magnitude;
                                KSPCSerialPort.VData.MNDeltaV = (float)ActiveVessel.patchedConicSolver.maneuverNodes[0].GetBurnVector(ActiveVessel.patchedConicSolver.maneuverNodes[0].patch).magnitude; //Added JS
                            }
                        }
                    }

                    //Debug.Log("KSPSerialIO: 5");

                    Quaternion attitude = updateHeadingPitchRollField(ActiveVessel);

                    KSPCSerialPort.VData.Roll = (float)((attitude.eulerAngles.z > 180) ? (attitude.eulerAngles.z - 360.0) : attitude.eulerAngles.z);
                    KSPCSerialPort.VData.Pitch = (float)((attitude.eulerAngles.x > 180) ? (360.0 - attitude.eulerAngles.x) : -attitude.eulerAngles.x);
                    KSPCSerialPort.VData.Heading = (float)attitude.eulerAngles.y;

                    KSPCSerialPort.ControlStatus((int)enumAG.SAS, ActiveVessel.ActionGroups[KSPActionGroup.SAS]);
                    KSPCSerialPort.ControlStatus((int)enumAG.RCS, ActiveVessel.ActionGroups[KSPActionGroup.RCS]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Light, ActiveVessel.ActionGroups[KSPActionGroup.Light]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Gear, ActiveVessel.ActionGroups[KSPActionGroup.Gear]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Brakes, ActiveVessel.ActionGroups[KSPActionGroup.Brakes]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Abort, ActiveVessel.ActionGroups[KSPActionGroup.Abort]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom01, ActiveVessel.ActionGroups[KSPActionGroup.Custom01]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom02, ActiveVessel.ActionGroups[KSPActionGroup.Custom02]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom03, ActiveVessel.ActionGroups[KSPActionGroup.Custom03]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom04, ActiveVessel.ActionGroups[KSPActionGroup.Custom04]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom05, ActiveVessel.ActionGroups[KSPActionGroup.Custom05]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom06, ActiveVessel.ActionGroups[KSPActionGroup.Custom06]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom07, ActiveVessel.ActionGroups[KSPActionGroup.Custom07]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom08, ActiveVessel.ActionGroups[KSPActionGroup.Custom08]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom09, ActiveVessel.ActionGroups[KSPActionGroup.Custom09]);
                    KSPCSerialPort.ControlStatus((int)enumAG.Custom10, ActiveVessel.ActionGroups[KSPActionGroup.Custom10]);

                    if (ActiveVessel.orbit.referenceBody != null)
                    {
                        KSPCSerialPort.VData.SOINumber = GetSOINumber(ActiveVessel.orbit.referenceBody.name);
                    }

                    KSPCSerialPort.VData.MaxOverHeat = GetMaxOverHeat(ActiveVessel);
                    KSPCSerialPort.VData.MachNumber = (float)ActiveVessel.mach;
                    KSPCSerialPort.VData.IAS = (float)ActiveVessel.indicatedAirSpeed;

                    KSPCSerialPort.VData.CurrentStage = (byte)StageManager.CurrentStage;
                    KSPCSerialPort.VData.TotalStage = (byte)StageManager.StageCount;

                    //target distance and velocity stuff                    

                    KSPCSerialPort.VData.TargetDist = 0;
                    KSPCSerialPort.VData.TargetV = 0;

                    if (TargetExists())
                    {
                        KSPCSerialPort.VData.TargetDist = (float)Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetVessel().transform.position, ActiveVessel.transform.position);
                        KSPCSerialPort.VData.TargetV = (float)FlightGlobals.ship_tgtVelocity.magnitude;
                    }


                    KSPCSerialPort.VData.NavballSASMode = (byte)(((int)FlightGlobals.speedDisplayMode + 1) << 4); //get navball speed display mode
                    if (ActiveVessel.ActionGroups[KSPActionGroup.SAS])
                    {
                        KSPCSerialPort.VData.NavballSASMode = (byte)(((int)FlightGlobals.ActiveVessel.Autopilot.Mode + 1) | KSPCSerialPort.VData.NavballSASMode);
                    }

                    #region debugjunk

                    /*
                      Debug.Log("KSPSerialIO: Stage " + KSPCSerialPort.VData.CurrentStage.ToString() + ' ' +
                      KSPCSerialPort.VData.TotalStage.ToString()); 
                      Debug.Log("KSPSerialIO: Overheat " + KSPCSerialPort.VData.MaxOverHeat.ToString());
                      Debug.Log("KSPSerialIO: Mach " + KSPCSerialPort.VData.MachNumber.ToString());
                      Debug.Log("KSPSerialIO: IAS " + KSPCSerialPort.VData.IAS.ToString());
                    
                      Debug.Log("KSPSerialIO: SOI " + ActiveVessel.orbit.referenceBody.name + KSPCSerialPort.VData.SOINumber.ToString());
                    
                      ScreenMessages.PostScreenMessage(KSPCSerialPort.VData.OxidizerS.ToString() + "/" + KSPCSerialPort.VData.OxidizerTotS +
                      "   " + KSPCSerialPort.VData.Oxidizer.ToString() + "/" + KSPCSerialPort.VData.OxidizerTot);
                    */
                    //KSPCSerialPort.VData.Roll = Mathf.Atan2(2 * (x * y + w * z), w * w + x * x - y * y - z * z) * 180 / Mathf.PI;
                    //KSPCSerialPort.VData.Pitch = Mathf.Atan2(2 * (y * z + w * x), w * w - x * x - y * y + z * z) * 180 / Mathf.PI;
                    //KSPCSerialPort.VData.Heading = Mathf.Asin(-2 * (x * z - w * y)) *180 / Mathf.PI;
                    //Debug.Log("KSPSerialIO: Roll    " + KSPCSerialPort.VData.Roll.ToString());
                    //Debug.Log("KSPSerialIO: Pitch   " + KSPCSerialPort.VData.Pitch.ToString());
                    //Debug.Log("KSPSerialIO: Heading " + KSPCSerialPort.VData.Heading.ToString());
                    //Debug.Log("KSPSerialIO: VOrbit" + KSPCSerialPort.VData.VOrbit.ToString());
                    //ScreenMessages.PostScreenMessage(ActiveVessel.ActionGroups[KSPActionGroup.RCS].ToString());
                    //Debug.Log("KSPSerialIO: MNTime" + KSPCSerialPort.VData.MNTime.ToString() + " MNDeltaV" + KSPCSerialPort.VData.MNDeltaV.ToString());
                    //Debug.Log("KSPSerialIO: Time" + KSPCSerialPort.VData.MissionTime.ToString() + " Delta Time" + KSPCSerialPort.VData.deltaTime.ToString());
                    //Debug.Log("KSPSerialIO: Throttle = " + KSPCSerialPort.CPacket.Throttle.ToString());
                    //ScreenMessages.PostScreenMessage(KSPCSerialPort.VData.Fuelp.ToString());
                    //ScreenMessages.PostScreenMessage(KSPCSerialPort.VData.RAlt.ToString());
                    //KSPCSerialPort.Port.WriteLine("Success!");
                    /*
                    ScreenMessages.PostScreenMessage(KSPCSerialPort.VData.LiquidFuelS.ToString() + "/" + KSPCSerialPort.VData.LiquidFuelTotS +
                        "   " + KSPCSerialPort.VData.LiquidFuel.ToString() + "/" + KSPCSerialPort.VData.LiquidFuelTot);
                    
                    ScreenMessages.PostScreenMessage("MNTime " + KSPCSerialPort.VData.MNTime.ToString() + " MNDeltaV " + KSPCSerialPort.VData.MNDeltaV.ToString());
                    ScreenMessages.PostScreenMessage("TargetDist " + KSPCSerialPort.VData.TargetDist.ToString() + " TargetV " + KSPCSerialPort.VData.TargetV.ToString());
                     */
                    #endregion
                    KSPCSerialPort.sendPacket(KSPCSerialPort.VData);
                } //end refresh
                #endregion
                #region inputs
                if (KSPCSerialPort.ControlReceived)
                {
                    /*

                    ScreenMessages.PostScreenMessage("Nav Mode " + KSPCSerialPort.CPacket.NavballSASMode.ToString());
                    
                     ScreenMessages.PostScreenMessage("SAS: " + KSPCSerialPort.VControls.SAS.ToString() +
                     ", RCS: " + KSPCSerialPort.VControls.RCS.ToString() +
                     ", Lights: " + KSPCSerialPort.VControls.Lights.ToString() +
                     ", Gear: " + KSPCSerialPort.VControls.Gear.ToString() +
                     ", Brakes: " + KSPCSerialPort.VControls.Brakes.ToString() +
                     ", Precision: " + KSPCSerialPort.VControls.Precision.ToString() +
                     ", Abort: " + KSPCSerialPort.VControls.Abort.ToString() +
                     ", Stage: " + KSPCSerialPort.VControls.Stage.ToString(), 10f, KSPIOScreenStyle);
                    
                     Debug.Log("KSPSerialIO: SAS: " + KSPCSerialPort.VControls.SAS.ToString() +
                     ", RCS: " + KSPCSerialPort.VControls.RCS.ToString() +
                     ", Lights: " + KSPCSerialPort.VControls.Lights.ToString() +
                     ", Gear: " + KSPCSerialPort.VControls.Gear.ToString() +
                     ", Brakes: " + KSPCSerialPort.VControls.Brakes.ToString() +
                     ", Precision: " + KSPCSerialPort.VControls.Precision.ToString() +
                     ", Abort: " + KSPCSerialPort.VControls.Abort.ToString() +
                     ", Stage: " + KSPCSerialPort.VControls.Stage.ToString());
                     */

                    //if (FlightInputHandler.RCSLock != KSPCSerialPort.VControls.RCS)
                    if (KSPCSerialPort.VControls.RCS != KSPCSerialPort.VControlsOld.RCS)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.RCS, KSPCSerialPort.VControls.RCS);
                        KSPCSerialPort.VControlsOld.RCS = KSPCSerialPort.VControls.RCS;
                        //ScreenMessages.PostScreenMessage("RCS: " + KSPCSerialPort.VControls.RCS.ToString(), 10f, KSPIOScreenStyle);
                    }

                    //if (ActiveVessel.ctrlState.killRot != KSPCSerialPort.VControls.SAS)
                    if (KSPCSerialPort.VControls.SAS != KSPCSerialPort.VControlsOld.SAS)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, KSPCSerialPort.VControls.SAS);
                        KSPCSerialPort.VControlsOld.SAS = KSPCSerialPort.VControls.SAS;
                        //ScreenMessages.PostScreenMessage("SAS: " + KSPCSerialPort.VControls.SAS.ToString(), 10f, KSPIOScreenStyle);
                    }

                    if (KSPCSerialPort.VControls.Lights != KSPCSerialPort.VControlsOld.Lights)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Light, KSPCSerialPort.VControls.Lights);
                        KSPCSerialPort.VControlsOld.Lights = KSPCSerialPort.VControls.Lights;
                    }

                    if (KSPCSerialPort.VControls.Gear != KSPCSerialPort.VControlsOld.Gear)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Gear, KSPCSerialPort.VControls.Gear);
                        KSPCSerialPort.VControlsOld.Gear = KSPCSerialPort.VControls.Gear;
                    }

                    if (KSPCSerialPort.VControls.Brakes != KSPCSerialPort.VControlsOld.Brakes)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, KSPCSerialPort.VControls.Brakes);
                        KSPCSerialPort.VControlsOld.Brakes = KSPCSerialPort.VControls.Brakes;
                    }

                    if (KSPCSerialPort.VControls.Abort != KSPCSerialPort.VControlsOld.Abort)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Abort, KSPCSerialPort.VControls.Abort);
                        KSPCSerialPort.VControlsOld.Abort = KSPCSerialPort.VControls.Abort;
                    }

                    if (KSPCSerialPort.VControls.Stage != KSPCSerialPort.VControlsOld.Stage)
                    {
                        if (KSPCSerialPort.VControls.Stage)
                            StageManager.ActivateNextStage();

                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Stage, KSPCSerialPort.VControls.Stage);
                        KSPCSerialPort.VControlsOld.Stage = KSPCSerialPort.VControls.Stage;
                    }

                    //================ control groups

                    if (KSPCSerialPort.VControls.ControlGroup[1] != KSPCSerialPort.VControlsOld.ControlGroup[1])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom01, KSPCSerialPort.VControls.ControlGroup[1]);
                        KSPCSerialPort.VControlsOld.ControlGroup[1] = KSPCSerialPort.VControls.ControlGroup[1];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[2] != KSPCSerialPort.VControlsOld.ControlGroup[2])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom02, KSPCSerialPort.VControls.ControlGroup[2]);
                        KSPCSerialPort.VControlsOld.ControlGroup[2] = KSPCSerialPort.VControls.ControlGroup[2];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[3] != KSPCSerialPort.VControlsOld.ControlGroup[3])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom03, KSPCSerialPort.VControls.ControlGroup[3]);
                        KSPCSerialPort.VControlsOld.ControlGroup[3] = KSPCSerialPort.VControls.ControlGroup[3];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[4] != KSPCSerialPort.VControlsOld.ControlGroup[4])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom04, KSPCSerialPort.VControls.ControlGroup[4]);
                        KSPCSerialPort.VControlsOld.ControlGroup[4] = KSPCSerialPort.VControls.ControlGroup[4];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[5] != KSPCSerialPort.VControlsOld.ControlGroup[5])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom05, KSPCSerialPort.VControls.ControlGroup[5]);
                        KSPCSerialPort.VControlsOld.ControlGroup[5] = KSPCSerialPort.VControls.ControlGroup[5];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[6] != KSPCSerialPort.VControlsOld.ControlGroup[6])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom06, KSPCSerialPort.VControls.ControlGroup[6]);
                        KSPCSerialPort.VControlsOld.ControlGroup[6] = KSPCSerialPort.VControls.ControlGroup[6];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[7] != KSPCSerialPort.VControlsOld.ControlGroup[7])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom07, KSPCSerialPort.VControls.ControlGroup[7]);
                        KSPCSerialPort.VControlsOld.ControlGroup[7] = KSPCSerialPort.VControls.ControlGroup[7];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[8] != KSPCSerialPort.VControlsOld.ControlGroup[8])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom08, KSPCSerialPort.VControls.ControlGroup[8]);
                        KSPCSerialPort.VControlsOld.ControlGroup[8] = KSPCSerialPort.VControls.ControlGroup[8];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[9] != KSPCSerialPort.VControlsOld.ControlGroup[9])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom09, KSPCSerialPort.VControls.ControlGroup[9]);
                        KSPCSerialPort.VControlsOld.ControlGroup[9] = KSPCSerialPort.VControls.ControlGroup[9];
                    }

                    if (KSPCSerialPort.VControls.ControlGroup[10] != KSPCSerialPort.VControlsOld.ControlGroup[10])
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.Custom10, KSPCSerialPort.VControls.ControlGroup[10]);
                        KSPCSerialPort.VControlsOld.ControlGroup[10] = KSPCSerialPort.VControls.ControlGroup[10];
                    }

                    //Set sas mode
                    if (KSPCSerialPort.VControls.SASMode != KSPCSerialPort.VControlsOld.SASMode)
                    {
                        if (KSPCSerialPort.VControls.SASMode != 0 && KSPCSerialPort.VControls.SASMode < 11)
                        {
                            if (!ActiveVessel.Autopilot.CanSetMode((VesselAutopilot.AutopilotMode)(KSPCSerialPort.VControls.SASMode - 1)))
                            {
                                ScreenMessages.PostScreenMessage("KSPSerialIO: SAS mode " + KSPCSerialPort.VControls.SASMode.ToString() + " not avalible");
                            }
                            else
                            {
                                ActiveVessel.Autopilot.SetMode((VesselAutopilot.AutopilotMode)KSPCSerialPort.VControls.SASMode - 1);
                            }
                        }
                        KSPCSerialPort.VControlsOld.SASMode = KSPCSerialPort.VControls.SASMode;
                    }

                    //set navball mode
                    if (KSPCSerialPort.VControls.SpeedMode != KSPCSerialPort.VControlsOld.SpeedMode)
                    {
                        if (!((KSPCSerialPort.VControls.SpeedMode == 0) || ((KSPCSerialPort.VControls.SpeedMode == 3) && !TargetExists())))
                        {
                            FlightGlobals.SetSpeedMode((FlightGlobals.SpeedDisplayModes)(KSPCSerialPort.VControls.SpeedMode - 1));
                        }
                        KSPCSerialPort.VControlsOld.SpeedMode = KSPCSerialPort.VControls.SpeedMode;
                    }


                    // temporarily disengage SAS while steering
                    if (Math.Abs(KSPCSerialPort.VControls.Pitch) > KSPCSettings.SASTol ||
                    Math.Abs(KSPCSerialPort.VControls.Roll) > KSPCSettings.SASTol ||
                    Math.Abs(KSPCSerialPort.VControls.Yaw) > KSPCSettings.SASTol)
                    {
                        ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
                    }
                    else
                    {
                        if (KSPCSerialPort.VControls.SAS == true)
                        {
                            ActiveVessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                        }
                    }

                    KSPCSerialPort.ControlReceived = false;
                } //end ControlReceived
                #endregion


            }//end if null and same vessel
            else
            {
                //Debug.Log("KSPSerialIO: ActiveVessel not found");
            }

        }

        #region utilities

        private Boolean TargetExists()
        {
            return (FlightGlobals.fetch.VesselTarget != null) && (FlightGlobals.fetch.VesselTarget.GetVessel() != null); //&& is short circuiting
        }

        private byte GetMaxOverHeat(Vessel V)
        {
            byte percent = 0;
            double sPercent = 0, iPercent = 0;
            double percentD = 0, percentP = 0;

            foreach (Part p in ActiveVessel.parts)
            {
                //internal temperature
                iPercent = p.temperature / p.maxTemp;
                //skin temperature
                sPercent = p.skinTemperature / p.skinMaxTemp;

                if (iPercent > sPercent)
                    percentP = iPercent;
                else
                    percentP = sPercent;

                if (percentD < percentP)
                    percentD = percentP;
            }

            percent = (byte)Math.Round(percentD * 100);
            return percent;
        }


        private IOResource GetResourceTotal(Vessel V, string resourceName)
        {
            IOResource R = new IOResource();

            foreach (Part p in V.parts)
            {
                foreach (PartResource pr in p.Resources)
                {
                    if (pr.resourceName.Equals(resourceName))
                    {
                        R.Current += (float)pr.amount;
                        R.Max += (float)pr.maxAmount;

                        break;
                    }
                }
            }

            if (R.Max == 0)
                R.Current = 0;

            return R;
        }

        private void AxisInput(FlightCtrlState s)
        {
            switch (KSPCSettings.ThrottleEnable)
            {
                case 1:
                    s.mainThrottle = KSPCSerialPort.VControls.Throttle;
                    break;
                case 2:
                    if (s.mainThrottle == 0)
                    {
                        s.mainThrottle = KSPCSerialPort.VControls.Throttle;
                    }
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.Throttle != 0)
                    {
                        s.mainThrottle = KSPCSerialPort.VControls.Throttle;
                    }
                    break;
                default:
                    break;

            }

            switch (KSPCSettings.PitchEnable)
            {
                case 1:
                    s.pitch = KSPCSerialPort.VControls.Pitch;
                    break;
                case 2:
                    if (s.pitch == 0)
                        s.pitch = KSPCSerialPort.VControls.Pitch;
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.Pitch != 0)
                        s.pitch = KSPCSerialPort.VControls.Pitch;
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.RollEnable)
            {
                case 1:
                    s.roll = KSPCSerialPort.VControls.Roll;
                    break;
                case 2:
                    if (s.roll == 0)
                        s.roll = KSPCSerialPort.VControls.Roll;
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.Roll != 0)
                        s.roll = KSPCSerialPort.VControls.Roll;
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.YawEnable)
            {
                case 1:
                    s.yaw = KSPCSerialPort.VControls.Yaw;
                    break;
                case 2:
                    if (s.yaw == 0)
                        s.yaw = KSPCSerialPort.VControls.Yaw;
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.Yaw != 0)
                        s.yaw = KSPCSerialPort.VControls.Yaw;
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.TXEnable)
            {
                case 1:
                    s.X = KSPCSerialPort.VControls.TX;
                    break;
                case 2:
                    if (s.X == 0)
                        s.X = KSPCSerialPort.VControls.TX;
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.TX != 0)
                        s.X = KSPCSerialPort.VControls.TX;
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.TYEnable)
            {
                case 1:
                    s.Y = KSPCSerialPort.VControls.TY;
                    break;
                case 2:
                    if (s.Y == 0)
                        s.Y = KSPCSerialPort.VControls.TY;
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.TY != 0)
                        s.Y = KSPCSerialPort.VControls.TY;
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.TZEnable)
            {
                case 1:
                    s.Z = KSPCSerialPort.VControls.TZ;
                    break;
                case 2:
                    if (s.Z == 0)
                        s.Z = KSPCSerialPort.VControls.TZ;
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.TZ != 0)
                        s.Z = KSPCSerialPort.VControls.TZ;
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.WheelSteerEnable)
            {
                case 1:
                    s.wheelSteer = KSPCSerialPort.VControls.WheelSteer;
                    break;
                case 2:
                    if (s.wheelSteer == 0)
                    {
                        s.wheelSteer = KSPCSerialPort.VControls.WheelSteer;
                    }
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.WheelSteer != 0)
                    {
                        s.wheelSteer = KSPCSerialPort.VControls.WheelSteer;
                    }
                    break;
                default:
                    break;
            }

            switch (KSPCSettings.WheelThrottleEnable)
            {
                case 1:
                    s.wheelThrottle = KSPCSerialPort.VControls.WheelThrottle;
                    break;
                case 2:
                    if (s.wheelThrottle == 0)
                    {
                        s.wheelThrottle = KSPCSerialPort.VControls.WheelThrottle;
                    }
                    break;
                case 3:
                    if (KSPCSerialPort.VControls.WheelThrottle != 0)
                    {
                        s.wheelThrottle = KSPCSerialPort.VControls.WheelThrottle;
                    }
                    break;
                default:
                    break;
            }
        }

        private byte GetSOINumber(string name)
        {
            byte SOI;

            switch (name.ToLower())
            {
                case "sun":
                    SOI = 100;
                    break;
                case "moho":
                    SOI = 110;
                    break;
                case "eve":
                    SOI = 120;
                    break;
                case "gilly":
                    SOI = 121;
                    break;
                case "kerbin":
                    SOI = 130;
                    break;
                case "mun":
                    SOI = 131;
                    break;
                case "minmus":
                    SOI = 132;
                    break;
                case "duna":
                    SOI = 140;
                    break;
                case "ike":
                    SOI = 141;
                    break;
                case "dres":
                    SOI = 150;
                    break;
                case "jool":
                    SOI = 160;
                    break;
                case "laythe":
                    SOI = 161;
                    break;
                case "vall":
                    SOI = 162;
                    break;
                case "tylo":
                    SOI = 163;
                    break;
                case "bop":
                    SOI = 164;
                    break;
                case "pol":
                    SOI = 165;
                    break;
                case "eeloo":
                    SOI = 170;
                    break;
                default:
                    SOI = 0;
                    break;
            }
            return SOI;
        }

        // this recursive stage look up stuff stolen and modified from KOS and others
        public static List<Part> GetListOfActivatedEngines(Vessel vessel)
        {
            var retList = new List<Part>();

            foreach (var part in vessel.Parts)
            {
                foreach (PartModule module in part.Modules)
                {
                    var engineModule = module as ModuleEngines;
                    if (engineModule != null)
                    {
                        if (engineModule.getIgnitionState)
                        {
                            retList.Add(part);
                        }
                    }

                    var engineModuleFx = module as ModuleEnginesFX;
                    if (engineModuleFx != null)
                    {
                        if (engineModuleFx.getIgnitionState)
                        {
                            retList.Add(part);
                        }
                    }
                }
            }

            return retList;
        }

        public static double ProspectForResource(String resourceName, List<Part> engines)
        {
            List<Part> visited = new List<Part>();
            double total = 0;

            foreach (var part in engines)
            {
                total += ProspectForResource(resourceName, part, ref visited);
            }

            return total;
        }

        public static double ProspectForResource(String resourceName, Part engine)
        {
            List<Part> visited = new List<Part>();

            return ProspectForResource(resourceName, engine, ref visited);
        }

        public static double ProspectForResource(String resourceName, Part part, ref List<Part> visited)
        {
            double ret = 0;

            if (visited.Contains(part))
            {
                return 0;
            }

            visited.Add(part);

            foreach (PartResource resource in part.Resources)
            {
                if (resource.resourceName.ToLower() == resourceName.ToLower())
                {
                    ret += resource.amount;
                }
            }

            foreach (AttachNode attachNode in part.attachNodes)
            {
                if (attachNode.attachedPart != null //if there is a part attached here
                    && attachNode.nodeType == AttachNode.NodeType.Stack //and the attached part is stacked (rather than surface mounted)
                    && (attachNode.attachedPart.fuelCrossFeed //and the attached part allows fuel flow
                        )
                    && !(part.NoCrossFeedNodeKey.Length > 0 //and this part does not forbid fuel flow
                         && attachNode.id.Contains(part.NoCrossFeedNodeKey))) // through this particular node
                {


                    ret += ProspectForResource(resourceName, attachNode.attachedPart, ref visited);
                }
            }

            return ret;
        }

        public static double ProspectForResourceMax(String resourceName, List<Part> engines)
        {
            List<Part> visited = new List<Part>();
            double total = 0;

            foreach (var part in engines)
            {
                total += ProspectForResourceMax(resourceName, part, ref visited);
            }

            return total;
        }

        public static double ProspectForResourceMax(String resourceName, Part engine)
        {
            List<Part> visited = new List<Part>();

            return ProspectForResourceMax(resourceName, engine, ref visited);
        }

        public static double ProspectForResourceMax(String resourceName, Part part, ref List<Part> visited)
        {
            double ret = 0;

            if (visited.Contains(part))
            {
                return 0;
            }

            visited.Add(part);

            foreach (PartResource resource in part.Resources)
            {
                if (resource.resourceName.ToLower() == resourceName.ToLower())
                {
                    ret += resource.maxAmount;
                }
            }

            foreach (AttachNode attachNode in part.attachNodes)
            {
                if (attachNode.attachedPart != null //if there is a part attached here
                    && attachNode.nodeType == AttachNode.NodeType.Stack //and the attached part is stacked (rather than surface mounted)
                    && (attachNode.attachedPart.fuelCrossFeed //and the attached part allows fuel flow
                        )
                    && !(part.NoCrossFeedNodeKey.Length > 0 //and this part does not forbid fuel flow
                         && attachNode.id.Contains(part.NoCrossFeedNodeKey))) // through this particular node
                {


                    ret += ProspectForResourceMax(resourceName, attachNode.attachedPart, ref visited);
                }
            }

            return ret;
        }

        //Borrowed from MechJeb2
        private Quaternion updateHeadingPitchRollField(Vessel v)
        {
            Vector3d CoM, north, up;
            Quaternion rotationSurface;
            CoM = v.CoM;
            up = (CoM - v.mainBody.position).normalized;
            north = Vector3d.Exclude(up, (v.mainBody.position + v.mainBody.transform.up * (float)v.mainBody.Radius) - CoM).normalized;
            rotationSurface = Quaternion.LookRotation(north, up);
            return Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(v.GetTransform().rotation) * rotationSurface);
        }

        #endregion

        void FixedUpdate()
        {
        }

        void OnDestroy()
        {
            if (KSPCSerialPort.Port.IsOpen)
            {
                KSPCSerialPort.PortCleanup();
                ScreenMessages.PostScreenMessage("Port closed", 10f, KSPIOScreenStyle);
            }
        }
    }
}
