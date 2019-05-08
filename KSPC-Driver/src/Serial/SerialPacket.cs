using System;
using System.Runtime.InteropServices;
using KSPCDriver;

namespace KSPCDriver.Serial
{
    internal enum SerialPacketState : byte
    {
        ACK, //ACK FLAG
        VERIFIER, // VERIFIER
        SIZE,// SIZE
        PAYLOAD, // PAYLOAD
        CHECKSUM, // CHECKSUM
        INVALID, // INVALID CHECKSUM
        DONE // DONE
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SerializedVesselControls
    {
        public byte MainControls;                  //SAS RCS Lights Gear Brakes Precision Abort Stage 
        public byte Mode;                          //0 = stage, 1 = docking, 2 = map
        public ushort ControlGroup;                //control groups 1-10 in 2 bytes
        public byte NavballSASMode;                //AutoPilot mode (See above for AutoPilot modes)(Ignored if the equal to zero or out of bounds (>10)) //Navball mode
        public byte AdditionalControlByte1;
        public short Pitch;                        //-1000 -> 1000
        public short Roll;                         //-1000 -> 1000
        public short Yaw;                          //-1000 -> 1000
        public short TX;                           //-1000 -> 1000
        public short TY;                           //-1000 -> 1000
        public short TZ;                           //-1000 -> 1000
        public short WheelSteer;                   //-1000 -> 1000
        public short Throttle;                     // 0 -> 1000
        public short WheelThrottle;                // 0 -> 1000
    };
    public class SerialPacketControl
    {
        //
        private byte[] _payload;
        private byte payloadLength;
        public SerialPacketControl(byte[] buff, byte buffLength)
        {
            _payload = buff;
            payloadLength = buffLength;
        }
        unsafe public bool isValid()
        {
            return (payloadLength == sizeof(SerializedVesselControls));
        }
        public VesselControls? parsedData()
        {
            return this._parsePayload();
        }

        //Parser
        private VesselControls? _parsePayload()
        {
            try
            {
                SerializedVesselControls _controlPacket = this._deserializePacketControl();
                // 
                VesselControls _control = new VesselControls();
                _control.SAS = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 0);
                _control.RCS = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 1);
                _control.Light = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 2);
                _control.Gear = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 3);
                _control.Brakes = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 4);
                _control.Abort = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 5);
                _control.Stage = Utils.GetBooleanFromByteAt(_controlPacket.MainControls, 6);
                _control.Pitch = (float)_controlPacket.Pitch / 1000.0F;
                _control.Roll = (float)_controlPacket.Roll / 1000.0F;
                _control.Yaw = (float)_controlPacket.Yaw / 1000.0F;
                _control.TX = (float)_controlPacket.TX / 1000.0F;
                _control.TY = (float)_controlPacket.TY / 1000.0F;
                _control.TZ = (float)_controlPacket.TZ / 1000.0F;
                _control.WheelSteer = (float)_controlPacket.WheelSteer / 1000.0F;
                _control.Throttle = (float)_controlPacket.Throttle / 1000.0F;
                _control.WheelThrottle = (float)_controlPacket.WheelThrottle / 1000.0F;
                _control.SASMode = (int)_controlPacket.NavballSASMode & 0x0F;
                _control.SpeedMode = (int)(_controlPacket.NavballSASMode >> 4);
                //for (int j = 1; j <= Definitions.CONTROL_GROUP_COUNT-1; j++)
                //{
                //    _control.ControlGroup[j] = Utils.BitMathUshort(_controlPacket.ControlGroup, j);
                //}
                return _control;
            } catch (Exception e)
            {
                Utils.PrintDebugMessage("Exception when decoding packet: " + e.ToString());
                return null;
            }
        }
        //

        private SerializedVesselControls _deserializePacketControl()
        {
            SerializedVesselControls _controlPacket = new SerializedVesselControls();
            int len = Marshal.SizeOf(_controlPacket);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(_payload, 0, i, len);
            _controlPacket = (SerializedVesselControls)Marshal.PtrToStructure(i, _controlPacket.GetType());
            Marshal.FreeHGlobal(i);
            return _controlPacket;
        }
    }

    public class SerialPacketData
    {
        private byte[] _payload;
        public SerialPacketData(VesselData data)
        {
            _payload = this._generatePacket(data);
        }
        public byte[] getData()
        {
            return _payload;
        }
        public int getDataLength()
        {
            return _payload.Length;
        }
        private byte[] _generatePacket(VesselData data)
        {
            byte[] parsedData = Utils.StructureToByteArray(data);
            byte size = (byte)parsedData.Length;
            byte checksum = Utils.packetChecksum(parsedData, size);
            //payload
            byte[] packet = new byte[size + 4];
            parsedData.CopyTo(packet, 3);
            //
            packet[0] = Definitions.PACKET_ACK;
            packet[1] = Definitions.PACKET_VERIFIER;
            packet[2] = size;
            packet[packet.Length - 1] = checksum;
            return packet;
        }
    }
}
