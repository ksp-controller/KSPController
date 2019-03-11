using System;
using System.Threading;
using KSPCDriver;
using System.IO;

namespace KSPCDriver.Serial
{
    public class SerialPortWorker
    {
        public bool keepAlive;
        private Stream _stream; //read stream
        private Thread _thread; //com thread
        //read
        private Mutex _controllerPacketMutex; //in packet access mutex
        private SerialPacketControl _lastControllerPacket;
        //write
        private Mutex _sendPacketMutex; //out packet access mutex
        private SerialPacketData _sendData; //data to be sent
        public SerialPortWorker(Stream stream)
        {
            this.keepAlive = true;
            _stream = stream;
            _sendPacketMutex = new Mutex();
            _controllerPacketMutex = new Mutex();
            _thread = new Thread(_run);
            _thread.Start();
        }
        public void stop()
        {
            if (_thread == null) return;
            _thread.Interrupt();
            _thread = null;
        }
        public object getLastControlRead()
        {
            object tmpRetValue;
            this._controllerPacketMutex.WaitOne();
            tmpRetValue = _lastControllerPacket.parsedData();
            this._controllerPacketMutex.ReleaseMutex();
            return tmpRetValue;
        }
        public void setSendData(SerialPacketData data)
        {
            _sendPacketMutex.WaitOne();
            _sendData = data;
            _sendPacketMutex.ReleaseMutex();
        }

        private void _run()
        {
            Utils.PrintScreenMessage("Serial thread is running..");
            while (this.keepAlive)
            {
                this._receivePacket();
                this._sendPacket();
            }
            Utils.PrintScreenMessage("Serial thread is shutting down..");
        }
        private void _receivePacket()
        {
            try
            {
                byte[] buffer = new byte[Definitions.MAX_PACKET_SIZE + 4]; //+4 for ack, verrifier, checksum and size
                this._stream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult _result) {
                    try
                    {
                        int packetLenght = this._stream.EndRead(_result);
                        byte[] localCopy = new byte[packetLenght];
                        //copy into local copy
                        Buffer.BlockCopy(buffer, 0, localCopy, 0, packetLenght);
                        //try to get packet
                        SerialPacketControl tmpPacket = new SerialPacketControl(localCopy, packetLenght);
                        if (tmpPacket.isValid())
                        {
                            this._controllerPacketMutex.WaitOne();
                            this._lastControllerPacket = tmpPacket;
                            this._controllerPacketMutex.ReleaseMutex();
                        }
                    }
                    catch (IOException execption)
                    {
                        Utils.PrintScreenMessage("Exception in reading data " + execption.ToString());
                    }
                }, null);
            }
            catch (InvalidOperationException execption)
            {
                Utils.PrintScreenMessage("Exception on opening read buffer " + execption.ToString());
                Thread.Sleep(500);
            }
        }
        private void _sendPacket()
        {
            _sendPacketMutex.WaitOne();
            if (this._sendData != null)
            {
                this._stream.Write(_sendData.getData(), 0, _sendData.getDataLength());
                this._sendData = null; //mark data as sent!
            }
            _sendPacketMutex.ReleaseMutex();
        }

    }
}
