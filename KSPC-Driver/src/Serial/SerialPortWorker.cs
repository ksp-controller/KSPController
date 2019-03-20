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
            if (!this.keepAlive) return;
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
                byte[] buffer = new byte[Definitions.MAX_PACKET_SIZE + 4]; //+4 for ack, verrifier, checksum and size
                this._stream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult _result) {
                    try
                    {
                        //Utils.PrintDebugMessage("RX!");
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
                        else
                        {
                            Utils.PrintDebugMessage("[ARDUINO]: " + System.Text.Encoding.UTF8.GetString(localCopy, 0, localCopy.Length));
                        }
                    }
                    catch (IOException exception)
                    {
                        Utils.PrintDebugMessage("Exception in reading data " + exception.ToString());
                        this.stop();
                    }
                }, null);
            }
            catch (InvalidOperationException exception)
            {
                Utils.PrintDebugMessage("Exception on opening read buffer " + exception.ToString());
                Thread.Sleep(50);
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
            Thread.Sleep(50);
        }
    }
}
