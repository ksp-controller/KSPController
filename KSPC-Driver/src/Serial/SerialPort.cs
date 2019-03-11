﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;

using Psimax.IO.Ports;
using KSP.IO;
using UnityEngine;
using KSP.UI.Screens;
//
using KSPCDriver.KSPBridge;

namespace KSPCDriver.Serial
{
    public class SerialPort
    {
        private Psimax.IO.Ports.SerialPort _com;
        private string _comTTYPath;
        private SerialPortWorker _worker;
        public SerialPort(string serialPath)
        {
            _comTTYPath = serialPath;
            //TODO CUSTOM BAUD RATE?
            _com = new Psimax.IO.Ports.SerialPort(_comTTYPath, 9600, Parity.None, 8, StopBits.One);
            _worker = null;
            if (this._startCom())
            {
                _worker = new SerialPortWorker(_com.BaseStream);
            }
        }
        private bool _startCom()
        {
            Utils.PrintScreenMessage("Trying to establish communication at " + this._comTTYPath);
            try {
                this._com.Open();
                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Utils.PrintScreenMessage(e.Message);
                return false;
            } return true;
        }
        public void updateState(KSPStateMachine state)
        {
            object _tmp = this._worker.getLastControlRead();
            if (_tmp != null)
            {
                state.setControlRead((VesselControls)_tmp);
            }
        }
        public bool isPortOpen()
        {
            return (this._com != null && this._worker != null && this._com.IsOpen);
        }

        public void sendData(VesselData data)
        {

            SerialPacketData outPacket = new SerialPacketData(data);
            this._worker.setSendData(outPacket);
        }
        public void close()
        {
            if (this._com != null && this._com.IsOpen)
            {
                this._com.Close();
                this._com.Dispose();
            }
            _worker.stop();
        }
    }
}
