using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KSPCDriver
{
    public class USBDetector
    {
        protected static readonly int platf;
        protected static readonly List<string> serial_usbs;

        static USBDetector()
        {
            serial_usbs = new List<string>();
            platf = (int)Environment.OSVersion.Platform;
        }

        public static string[] GetUSBNames()
        {
            // Are we in MacOS or Linux
            if (platf == 4 || platf == 128 || platf == 6)
            {
                string[] ttys = System.IO.Directory.GetFiles("/dev/");
                foreach (string dev in ttys)
                {
                    //Arduino MEGAs show up as ttyACMX in Linux or cu.usbmodemFAXXX in MacOS
                    //due to their different USB<->RS232 chips
                    if (dev.StartsWith("/dev/ttyUSB", StringComparison.CurrentCulture)
                        || dev.StartsWith("/dev/ttyACM", StringComparison.CurrentCulture)
                        || dev.StartsWith("/dev/cu.usb", StringComparison.CurrentCulture))
                    {
                        serial_usbs.Add(dev);
                    }
                }
            }
            return serial_usbs.ToArray();
        }
    }
}