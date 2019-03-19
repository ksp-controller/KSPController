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
using KSPCDriver.Serial;
using KSPCDriver.KSPBridge;

namespace KSPCDriver
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KSPControllerDriver : MonoBehaviour
    {
        private static KSPStateMachine _state;
        private static SerialController _serialController;
        //
        void Awake() { }
        //
        void Start()
        {
            Utils.PrintScreenMessage("KSPCDriver is starting..");
            _state = new KSPStateMachine();
            _serialController = new SerialController();
        }
        void Update()
        {

            if (FlightGlobals.ActiveVessel != null)
            {
               if (_serialController != null && _serialController.isCommunicationAvailable())
                {
                    _state.updateOnGameThread();
                    _serialController.sendVesselData(_state.getVesselData());
                }
                else
                {
                    Utils.PrintScreenMessage("Serial com not available at " + KSPCSettings.DefaultPort + ". Is the controller connected?");
                }
            }
            else
            {
                Utils.PrintScreenMessage("No active vessel?");
            }
        }
        void OnDestroy()
        {
            _serialController.close();
        }
    }
}