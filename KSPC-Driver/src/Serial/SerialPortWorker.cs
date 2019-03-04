using System;
using System.Threading;
using KSPCDriver;

namespace KSPCDriver.Serial
{
    internal enum SerialPacket : byte
    {
        ACK, //ACK FLAG
        VERIFIER, // VERIFIER
        SIZE,// SIZE
        PAYLOAD, // PAYLOAD
        CHECKSUM, // CHECKSUM
        NONE // INAVLID
    }

    public class SerialPortWorker
    {
        public bool keepAlive;
        private Thread _thread; //com thread
        private Stream _inStream; //read stream
        private SerialPacket _readState; //current packet read state
        public SerialPortThread(Stream _readStream)
        {
            this.keepAlive = true;
            inStream = _readStream;
            _thread = new Thread(_run);
            _thread.Start();
        }
        private void _run()
        {
            Utils.PrintScreenMessage("Serial thread started!");
            byte[] buffer = new byte[PAYLOAD_SIZE + 4];
            Action reader = delegate {
                try {
                    this._inStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult _result) {
                      try {
                            int packetLenght = this._inStream.EndRead(_result);
                            byte[] localCopy = new byte[packetLenght];
                            //copy into local copy
                            Buffer.BlockCopy(buffer, 0, localCopy, 0, packetLenght);
                            //handle packet
                            this._handleNewPacket(localCopy, packetLenght);
                      } catch (IOException execption) {
                            Utils.PrintScreenMessage("Exception in reading data " + exeception.ToString());
                        }
                    }, null);
                } catch (InvalidOperationException execption) {
                    Utils.PrintScreenMessage("Exception on opening read buffer " + execption.ToString());
                    Thread.Sleep(500);
                }
            };
            //keep reading til flag says to stop
            while (this.keepAlive) reader();
            Utils.PrintScreenMessage("Serial thread is shutting down!");
        }
        private void _handleNewPacket(byte[] ReadBuffer, int BufferLength)
        {
            for (int x = 0; x < BufferLength; x++) 
            {
                switch (this._readState)
                {
                    case ReceiveStates.FIRSTHEADER:
                        if (ReadBuffer[x] == 0xBE)
                        {
                            CurrentState = ReceiveStates.SECONDHEADER;
                        }
                        break;
                    case ReceiveStates.SECONDHEADER:
                        if (ReadBuffer[x] == 0xEF)
                        {
                            CurrentState = ReceiveStates.SIZE;
                        }
                        else
                        {
                            CurrentState = ReceiveStates.FIRSTHEADER;
                        }
                        break;
                    case ReceiveStates.SIZE:
                        CurrentPacketLength = ReadBuffer[x];
                        CurrentBytesRead = 0;
                        CurrentState = ReceiveStates.PAYLOAD;
                        break;
                    case ReceiveStates.PAYLOAD:
                        PayloadBuffer[CurrentBytesRead] = ReadBuffer[x];
                        CurrentBytesRead++;
                        if (CurrentBytesRead == CurrentPacketLength)
                        {
                            CurrentState = ReceiveStates.CS;
                        }
                        break;
                    case ReceiveStates.CS:
                        if (CompareChecksum(ReadBuffer[x]))
                        {
                            SerialMutex.WaitOne();
                            Buffer.BlockCopy(PayloadBuffer, 0, NewPacketBuffer, 0, CurrentBytesRead);
                            NewPacketFlag = true;
                            SerialMutex.ReleaseMutex();
                            // Seedy hack: Handshake happens during scene
                            // load before Update() is ever called
                            if (!DisplayFound)
                            {
                                InboundPacketHandler();
                            }
                        }
                        CurrentState = ReceiveStates.FIRSTHEADER;
                        break;
                }
            }
        }
    }
}
