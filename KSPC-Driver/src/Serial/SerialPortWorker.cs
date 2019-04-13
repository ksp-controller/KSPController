using System;
using System.Threading;
using KSPCDriver;
using System.IO;

namespace KSPCDriver.Serial
{
    unsafe public class SerialPortWorker
    {
        public bool keepAlive;
        private Stream _stream; //read stream
        //read
        private Mutex _controllerPacketMutex; //in packet access mutex
        private SerialPacketControl _lastControllerPacket;
        private Thread _inThread; //com thread
        //write
        private Mutex _sendPacketMutex; //out packet access mutex
        private SerialPacketData _sendData; //data to be sent
        private Thread _outThread;//
        public SerialPortWorker(Stream stream)
        {
            this.keepAlive = true;
            _stream = stream;
            _sendPacketMutex = new Mutex();
            _controllerPacketMutex = new Mutex();
            _inThread = new Thread(new ThreadStart(_runReceive));
            _outThread = new Thread(new ThreadStart(_runSend));
            _outThread.Start();
            _inThread.Start();
        }
        public void stop()
        {
            Utils.PrintDebugMessage("Stopping worker..");
            if (this.keepAlive == false) return;
            this.keepAlive = false;
            _inThread.Interrupt();
            _inThread = null;
            _outThread.Interrupt();
            _outThread = null;
        }
        public object getLastControlRead()
        {
            if (!this.keepAlive) return null;
            object tmpRetValue;
            this._controllerPacketMutex.WaitOne();
            tmpRetValue = _lastControllerPacket.parsedData();
            this._controllerPacketMutex.ReleaseMutex();
            return tmpRetValue;
        }
        public void setSendData(SerialPacketData data)
        {
            if (!this.keepAlive) return;
            this._sendPacketMutex.WaitOne();
            this._sendData = data;
            this._sendPacketMutex.ReleaseMutex();
        }
        private void _runSend()
        {
            Utils.PrintDebugMessage("Serial OUT thread is running..");
            while (this.keepAlive) this._sendPacket();
            Utils.PrintDebugMessage("Serial OUT thread is shutting down..");
        }
        private void _runReceive()
        {
            Utils.PrintDebugMessage("Serial IN thread is running..");
            while (this.keepAlive) this._receivePacket();
            Utils.PrintDebugMessage("Serial IN thread is shutting down..");
        }
        //
        private void _receivePacket()
        {
            try
            {
                //initiate read stuff
                byte _currReadSize = 0; //current packet size
                byte _currReadDone = 0; //current packet already read
                byte[] _readPayload = new byte[Definitions.MAX_PACKET_SIZE]; //tmp payload
                SerialPacketState _readState = SerialPacketState.ACK;
                //while can read, read byte is not -1 (INVALID) && thread should keep alive
                int _read;
                while (this._stream.CanRead && (_read = this._stream.ReadByte()) != -1 && this.keepAlive)
                {
                    switch (_readState)
                    {
                        case SerialPacketState.ACK: //packet header
                                if ((byte)_read == Definitions.PACKET_ACK) _readState = SerialPacketState.VERIFIER;
                            break;
                        case SerialPacketState.VERIFIER:
                                if ((byte)_read == Definitions.PACKET_VERIFIER) _readState = SerialPacketState.SIZE; //proceed
                                else _readState = SerialPacketState.ACK; //verifier failed, get back to ack
                            break;
                        case SerialPacketState.SIZE:
                                _currReadSize = (byte)_read;
                                _readState = SerialPacketState.PAYLOAD; //receive payload
                            break;
                        case SerialPacketState.PAYLOAD:
                                _readPayload[_currReadDone] = (byte)_read;
                                _currReadDone++;
                                if (_currReadDone == _currReadSize) _readState = SerialPacketState.CHECKSUM; //reach expected size, go to checksum
                            break;
                        case SerialPacketState.CHECKSUM:
                                {
                                   if (Utils.packetChecksum(_readPayload, _currReadDone) == (byte)_read)
                                    {
                                        this._controllerPacketMutex.WaitOne(); //possible dead lock here, should use try catch to do all the sutff below
                                        //get safe pointer for game thread usage!
                                        byte[] _localPacket = new byte[Definitions.MAX_PACKET_SIZE];
                                        Buffer.BlockCopy(_localPacket, 0, _readPayload, 0, _currReadSize);
                                        SerialPacketControl tmpPacket = new SerialPacketControl(_localPacket);
                                        //Check for validity
                                        if (tmpPacket.isValid()) this._lastControllerPacket = tmpPacket;
                                        else Utils.PrintDebugMessage("[ARDUINO]:: " + System.Text.Encoding.UTF8.GetString(_readPayload, 0, _currReadDone));
                                        //mark as done to allow next packet recieve
                                        _readState = SerialPacketState.DONE;
                                        this._controllerPacketMutex.ReleaseMutex();
                                    }
                                    else
                                    {
                                        Utils.PrintDebugMessage("[ARDUINO]:: " + System.Text.Encoding.UTF8.GetString(_readPayload, 0, _currReadDone));
                                        Utils.PrintDebugMessage("CHK: " + Utils.packetChecksum(_readPayload, _currReadDone).ToString() + " - " + _read.ToString());
                                        _readState = SerialPacketState.INVALID;
                                    }
                                }
                                
                            break;
                    }
                    //if need to reset
                    if (_readState == SerialPacketState.DONE || _readState == SerialPacketState.INVALID) {
                        _currReadSize = 0;
                        _currReadDone = 0; //current packet already read
                        Array.Clear(_readPayload, 0, _readPayload.Length);
                        _readState = SerialPacketState.ACK;
                        Thread.Sleep((int)Definitions.SERIAL_THREAD_FREQUENCY); //sleep for next packet
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                Utils.PrintDebugMessage("Exception when trying to open read buffer " + exception.ToString());
                Thread.Sleep((int)Definitions.SERIAL_THREAD_FREQUENCY);
                this.stop();
            }
        }
        private void _sendPacket()
        {
            this._sendPacketMutex.WaitOne();
            if (this._sendData != null)
            {
                //Utils.PrintDebugMessage("TX!");
                this._stream.Write(_sendData.getData(), 0, _sendData.getDataLength());
                this._sendData = null; //mark data as sent!
            }
            this._sendPacketMutex.ReleaseMutex();
            Thread.Sleep((int)Definitions.SERIAL_THREAD_FREQUENCY);
        }
    }
}
