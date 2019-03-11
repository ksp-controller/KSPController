using System;
using KSPCDriver;

namespace KSPCDriver.KSPBridge
{
    public class KSPStateMachine
    {
        public bool _inputUpdated, _validVessel;
        public /*VesselControls*/object _previousVesselRead, _lastVesselRead; //last state set and last read
        public Vessel _kspCurrentVessel;
        public object _kspCurrentVesselData;
        public KSPStateMachine()
        {
            _inputUpdated = _validVessel = false;
            _kspCurrentVessel = new Vessel();
            _previousVesselRead = _lastVesselRead = null;
        }
        public void setControlRead(VesselControls newRead)
        {
            _inputUpdated = true;
            _previousVesselRead = _lastVesselRead;
            _lastVesselRead = newRead;
        }
        public object getVesselData()
        {
            return _kspCurrentVesselData;
        }
        public void updateOnGameThread()
        {
            this._reloadCurrentKSPVessel();
            this._updateControllerInputOnVessel();
        }
        //
        private void _reloadCurrentKSPVessel()
        {
            if (_kspCurrentVessel.id != FlightGlobals.ActiveVessel.id)
            {
                //remove callback from previous vessel
                _kspCurrentVessel.OnPostAutopilotUpdate -= _updateControllerInputOnVesselCallback;
                //set new vessel
                _kspCurrentVessel = FlightGlobals.ActiveVessel;
                _kspCurrentVessel.OnPostAutopilotUpdate += _updateControllerInputOnVesselCallback;
                _validVessel = true;

                if (_lastVesselRead != null) _inputUpdated = true;
            }
        }

        public void _updateControllerInputOnVessel()
        {
            if (_lastVesselRead != null && _inputUpdated && _validVessel)
            {
                KSPVesselBridge.SetControllerOnVessel((VesselControls)_lastVesselRead, (VesselControls)_previousVesselRead, _kspCurrentVessel);
                _inputUpdated = false;
            }
            if (_validVessel)
            {
                _kspCurrentVesselData = KSPVesselBridge.GetVesselData(_kspCurrentVessel);
            }
        }
        public void _updateControllerInputOnVesselCallback(FlightCtrlState s)
        {
            KSPVesselBridge.FillVesselStateWithController(s, (VesselControls)_lastVesselRead);
        }
    }
}
