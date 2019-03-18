using System;
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
            _currentConnection = new SerialPort(controllerTTY);
            if (_currentConnection != null) {
                //try to establish connection
                if (_currentConnection.isPortOpen()) Utils.PrintScreenMessage("Connection established with controller at " + controllerTTY);
                else Utils.PrintScreenMessage("Connection FAIL to be established with controller at " + controllerTTY);
            }
            else Utils.PrintScreenMessage("Cant find controller.");
        }
        public void sendVesselData(object vData)
        {
            if (vData != null)
            {
                this._currentConnection.sendData((VesselData)vData);
            }
        }
        public void close()
        {
            if (this.isCommunicationAvailable())
            {
                this._currentConnection.close();
                Utils.PrintScreenMessage("Closing port with controller at " + this._getControllerTTY());
            }
        }
        public bool isCommunicationAvailable()
        {
            return (this._currentConnection != null && this._currentConnection.isPortOpen());
        }
    }
}
