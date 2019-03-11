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
    public class KSPSerialIO : MonoBehaviour
    {
        private KSPStateMachine _state;
        private SerialController _serialController;
        //
        void Awake() 
        { 
            Utils.PrintScreenMessage("KSPCDriver is awake");
        }
        //
        void Start()
        {
            _state = new KSPStateMachine();
            _serialController = new SerialController();

        }
        void Update()
        {

            if (FlightGlobals.ActiveVessel != null)
            {
                if (_serialController.isCommunicationAvailable())
                {
                    this._state.updateOnGameThread();
                    this._serialController.sendVesselData(this._state.getVesselData());
                }
                else
                {
                    Utils.PrintScreenMessage("Serial com not available at " + KSPCSettings.DefaultPort + ".\n Is the controller connected?");
                }
            }
        }
        void OnDestroy()
        {
            this._serialController.close();
        }
    }
}



//public static void ControlStatus(int n, Boolean s)
//{
//    if (s)
//        VData.ActionGroups |= (UInt16)(1 << n);       // forces nth bit of x to be 1.  all other bits left alone.
//    else
//        VData.ActionGroups &= (UInt16)~(1 << n);      // forces nth bit of x to be 0.  all other bits left alone.
//}