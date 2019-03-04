using System;
using KSPCDriver;
namespace KSPCDriver.KSPBridge
{
    public class KSPStateMachine
    {
        public VesselControls _lastVesselRead; //last packet read
        public VesselControls _lastVesselState; //last state that was set 

        public KSPStateMachine()
        {
            _lastVesselRead = new VesselControls();
            _lastVesselRead.ControlGroup = new Boolean[Definitions.CONTROL_GROUP_COUNT];
            _lastVesselState = new VesselControls();
            _lastVesselState.ControlGroup = new Boolean[Definitions.CONTROL_GROUP_COUNT];
        }
    }
}
