using System;
using KSPCDriver.Utils;
using KSPCDriver.Serial;

namespace KSPCDriver.Serial
{
    public class SerialController
    {
        private SerialPort _currentConnection;
        public SerialController()
        {
            this._openPort();
        }
        private string _getControllerTTY()
        {
            return KSPCSettings.DefaultPort;
        }
        private void _openPort()
        {
            Utils.PrintScreenMessage("Starting serial communication..");
            string controllerTTY = this._getControllerTTY();
            //try to initialize port with specified tty
            if ((_currentConnection = new SerialPort(controllerTTY)) == null) {
                //try to establish connection
                if (_currentConnection.open()) Utils.PrintScreenMessage("Connection established with controller at " + controllerTTY);
                else Utils.PrintScreenMessage("Connection FAIL to be established with controller at " + controllerTTY);
            }
            else Utils.PrintScreenMessage("Cant find controller.");
        }


        public Boolean isCommunicationAvailable()
        {

        }
    }
}
