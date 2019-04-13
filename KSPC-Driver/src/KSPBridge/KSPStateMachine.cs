using System;
using KSPCDriver;

namespace KSPCDriver.KSPBridge
{
    public class KSPStateMachine
    {
        public bool _validVessel;
        public VesselControls? _previousVesselRead, _lastVesselRead; //last state set and last read
        public Vessel _kspCurrentVessel;
        public VesselData? _kspCurrentVesselData;
        public KSPStateMachine()
        {
            _validVessel = false;
            _kspCurrentVessel = new Vessel();
            _previousVesselRead = _lastVesselRead = null;
        }
        public void setControlRead(VesselControls? newRead)
        {
            _previousVesselRead = _lastVesselRead;
            _lastVesselRead = newRead;
        }
        public VesselData? getVesselData()
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
            }
            if (_validVessel)
            {
                _kspCurrentVesselData = KSPVesselBridge.GetVesselData(_kspCurrentVessel);
            }
        }

        public void _updateControllerInputOnVessel()
        {
            if (_lastVesselRead != null && _previousVesselRead != null && _validVessel)
            {
                KSPVesselBridge.SetControllerOnVessel((VesselControls)_lastVesselRead, (VesselControls)_previousVesselRead, _kspCurrentVessel);
            }
        }
        public void _updateControllerInputOnVesselCallback(FlightCtrlState s)
        {
            if (_lastVesselRead != null && _validVessel)
            {
                KSPVesselBridge.FillVesselStateWithController(s, (VesselControls)_lastVesselRead);
            }

        }
    }
}
