using System;
using System.Collections.Generic;
using KSP.IO;
using UnityEngine;
using KSP.UI.Screens;
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
            Utils.PrintScreenMessage("KSPCDriver: Loading settings...");

            cfg.load();
            DefaultPort = cfg.GetValue<string>("DefaultPort");
            print("KSPCDriver: Default Port = " + DefaultPort);

            BaudRate = cfg.GetValue<int>("BaudRate");
            print("KSPCDriver: BaudRate = " + BaudRate.ToString());

            PitchEnable = cfg.GetValue<bool>("PitchEnable");
            print("KSPCDriver: Pitch Enable = " + PitchEnable.ToString());

            RollEnable = cfg.GetValue<bool>("RollEnable");
            print("KSPCDriver: Roll Enable = " + RollEnable.ToString());

            YawEnable = cfg.GetValue<bool>("YawEnable");
            print("KSPCDriver: Yaw Enable = " + YawEnable.ToString());

            TXEnable = cfg.GetValue<bool>("TXEnable");
            print("KSPCDriver: Translate X Enable = " + TXEnable.ToString());

            TYEnable = cfg.GetValue<bool>("TYEnable");
            print("KSPCDriver: Translate Y Enable = " + TYEnable.ToString());

            TZEnable = cfg.GetValue<bool>("TZEnable");
            print("KSPCDriver: Translate Z Enable = " + TZEnable.ToString());

            WheelSteerEnable = cfg.GetValue<bool>("WheelSteerEnable");
            print("KSPCDriver: Wheel Steering Enable = " + WheelSteerEnable.ToString());

            ThrottleEnable = cfg.GetValue<bool>("ThrottleEnable");
            print("KSPCDriver: Throttle Enable = " + ThrottleEnable.ToString());

            WheelThrottleEnable = cfg.GetValue<bool>("WheelThrottleEnable");
            print("KSPCDriver: Wheel Throttle Enable = " + WheelThrottleEnable.ToString());

            SASTol = cfg.GetValue<double>("SASTol");
            print("KSPCDriver: SAS Tol = " + SASTol.ToString());
        }
    }
}
