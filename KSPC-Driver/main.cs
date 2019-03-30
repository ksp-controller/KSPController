using UnityEngine;

using KSPCDriver.Serial;
using KSPCDriver.KSPBridge;

namespace KSPCDriver
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KSPControllerDriver :MonoBehaviour
    {
        private static KSPStateMachine _state;
        private static SerialController _serialController;

        //
        void Awake() { }
        //
        void Start()
        {
            Utils.PrintScreenMessage("KSPCDriver is starting..");
            _state = new KSPStateMachine();
            _serialController = new SerialController();
        }
        void Update()
        {

            if (FlightGlobals.ActiveVessel != null)
            {
               if (_serialController != null && _serialController.isCommunicationAvailable())
                {
                    _state.updateOnGameThread();
                    _serialController.sendVesselData(_state.getVesselData());
                }
                else
                {
                    Utils.PrintScreenMessage("Serial port at " + KSPCSettings.DefaultPort + " is not working. Is the controller connected?");
                }
            }
            else
            {
                Utils.PrintScreenMessage("No active vessel?");
            }
        }
        void OnDestroy()
        {
            _serialController.close();
        }
    }
}