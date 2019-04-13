using System;
using KSPCDriver.KSPBridge;
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
            //try to initialize port with specified tty
            _currentConnection = new SerialPort(this._getControllerTTY());
            if (_currentConnection != null) {
                //try to establish connection
                if (_currentConnection.isPortOpen()) Utils.PrintScreenMessage("Established conn. at " + this._getControllerTTY());
                else Utils.PrintScreenMessage("FAIL conn. at " + this._getControllerTTY());
            }
            else Utils.PrintScreenMessage("Cant find controller.");
        }
        public void sendVesselData(VesselData? vData)
        {
            if (vData != null)
            {
                this._currentConnection.sendData((VesselData)vData);
            }
        }
        public void updateState(KSPStateMachine state)
        {
            this._currentConnection.updateState(state);
        }
        public void close()
        {
            if (this._currentConnection != null)
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
