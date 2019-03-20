using System;
using KSPCDriver;

namespace KSPCDriver.KSPBridge
{
    public class KSPStateMachine
    {
        public bool _v1NeedsUpdate, _v2NeedsUpdate, _validVessel;
        public /*VesselControls*/object _previousVesselRead, _lastVesselRead; //last state set and last read
        public Vessel _kspCurrentVessel;
        public object _kspCurrentVesselData;
        public KSPStateMachine()
        {
            _v1NeedsUpdate = _v2NeedsUpdate = _validVessel = false;
            _kspCurrentVessel = new Vessel();
            _previousVesselRead = _lastVesselRead = null;
        }
        public void setControlRead(VesselControls newRead)
        {
            _v1NeedsUpdate = _v2NeedsUpdate = true;
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
                Utils.PrintScreenMessage("Vessel change detected!");
                //remove callback from previous vessel
                _kspCurrentVessel.OnPostAutopilotUpdate -= this._updateControllerInputOnVesselCallback;
                //set new vessel
                _kspCurrentVessel = FlightGlobals.ActiveVessel;
                _kspCurrentVessel.OnPostAutopilotUpdate += this._updateControllerInputOnVesselCallback;
                _validVessel = true;
                //
                if (_lastVesselRead != null) _v1NeedsUpdate = _v2NeedsUpdate = true;
            }
            if (_validVessel)
            {
                _kspCurrentVesselData = KSPVesselBridge.GetVesselData(_kspCurrentVessel);
            }
        }

        public void _updateControllerInputOnVessel()
        {
            if (_lastVesselRead != null && _v1NeedsUpdate && _validVessel)
            {
                KSPVesselBridge.SetControllerOnVessel((VesselControls)_lastVesselRead, (VesselControls)_previousVesselRead, _kspCurrentVessel);
                _v1NeedsUpdate = false;
            }
        }
        public void _updateControllerInputOnVesselCallback(FlightCtrlState s)
        {
            if (_lastVesselRead != null && _v2NeedsUpdate && _validVessel)
            {
                KSPVesselBridge.FillVesselStateWithController(s, (VesselControls)_lastVesselRead);
                _v2NeedsUpdate = false;
            }

        }
    }
}
