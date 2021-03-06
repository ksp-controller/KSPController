﻿using KSP.IO;
using UnityEngine;

namespace KSPCDriver
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class KSPCSettings : MonoBehaviour
    {
        //configurator 
        public static PluginConfiguration cfg = PluginConfiguration.CreateForType<KSPCSettings>();
        //
        public static string DefaultPort;
        public static int BaudRate;
        //
        public static bool PitchEnable;
        public static bool RollEnable;
        public static bool YawEnable;
        public static bool TXEnable;
        public static bool TYEnable;
        public static bool TZEnable;
        public static bool WheelSteerEnable;
        public static bool ThrottleEnable;
        public static bool WheelThrottleEnable;
        public static double SASTol;
        //
        void Awake()
        {
            Utils.PrintDebugMessage("KSPCDriver: Loading settings...");
            cfg.load();
            //Read the default port from config file
            DefaultPort = cfg.GetValue<string>("DefaultPort");
            Utils.PrintDebugMessage("KSPCDriver: Default Port = " + DefaultPort);
            //Check if have a USB port from detector
            string usbPort = USBDetector.GetControllerPort();
            if (usbPort != null)
            {
                DefaultPort = usbPort;
                Utils.PrintDebugMessage("KSPCDriver: Using USB Port = " + DefaultPort);
            }

            BaudRate = cfg.GetValue<int>("BaudRate");
            Utils.PrintDebugMessage("KSPCDriver: BaudRate = " + BaudRate.ToString());

            PitchEnable = cfg.GetValue<int>("PitchEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Pitch Enable = " + PitchEnable.ToString());

            RollEnable = cfg.GetValue<int>("RollEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Roll Enable = " + RollEnable.ToString());

            YawEnable = cfg.GetValue<int>("YawEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Yaw Enable = " + YawEnable.ToString());

            TXEnable = cfg.GetValue<int>("TXEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Translate X Enable = " + TXEnable.ToString());

            TYEnable = cfg.GetValue<int>("TYEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Translate Y Enable = " + TYEnable.ToString());

            TZEnable = cfg.GetValue<int>("TZEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Translate Z Enable = " + TZEnable.ToString());

            WheelSteerEnable = cfg.GetValue<int>("WheelSteerEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Wheel Steering Enable = " + WheelSteerEnable.ToString());

            ThrottleEnable = cfg.GetValue<int>("ThrottleEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Throttle Enable = " + ThrottleEnable.ToString());

            WheelThrottleEnable = cfg.GetValue<int>("WheelThrottleEnable") == 1;
            Utils.PrintDebugMessage("KSPCDriver: Wheel Throttle Enable = " + WheelThrottleEnable.ToString());

            SASTol = cfg.GetValue<double>("SASTol");
            Utils.PrintDebugMessage("KSPCDriver: SAS Tol = " + SASTol.ToString());
        }
        public static void reloadUSBPorts()
        {
            //Check if have a USB port from detector
            string usbPort = USBDetector.GetControllerPort();
            if (usbPort != null)
            {
                DefaultPort = usbPort;
                Utils.PrintDebugMessage("KSPCDriver: Using USB Port = " + DefaultPort);
            }
        }
    }
}
