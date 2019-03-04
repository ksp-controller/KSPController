using System;
using KSP.UI.Screens;
using UnityEngine;

namespace KSPCDriver
{
    public class Utils
    {
        private static ScreenMessageStyle DEFAULTMESSAGETYPE = ScreenMessageStyle.UPPER_RIGHT;

        public static void PrintScreenMessage(string message)
        {
            ScreenMessages.PostScreenMessage(message, 10f, DEFAULTMESSAGETYPE);
        }
        public static void PrintDebugMessage(string message)
        {
            Debug.Log(message);
        }
    }
}
