using System;
using System.Timers;
using System.IO;
using RJCP.IO.Ports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RFController {
    public class MTRF {
        Queue<Buf> queue = new Queue<Buf>();
        public event EventHandler NewDataReceived;
        public event EventHandler DataSent;
        SerialPortStream serialPort;
        public Buf rxBuf;
        public Buf txBuf;
        public string ConnectedPortName {
            get {
                if (serialPort != null && serialPort.IsOpen) {
                    return serialPort.PortName;
                } else {
                    return "Not connected";
                }
            }
        }

        public float[] LastTempBuf { get; private set; }

        Timer CmdQueueTmr;
        Task task1;
        Task<List<Mtrf>> task2;

        System.Threading.ManualResetEvent AnswerReceived = new System.Threading.ManualResetEvent(true);
        System.Threading.ManualResetEvent AnswerReceived2 = new System.Threading.ManualResetEvent(false);

        public MTRF() {
            serialPort = new SerialPortStream {
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None
            };
            serialPort.DataReceived += new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
            rxBuf = new Buf();
            txBuf = new Buf();

            LastTempBuf = new float[64];
            for (int i = 0; i < LastTempBuf.Length; i++) {
                LastTempBuf[i] = 65535;
            }

            CmdQueueTmr = new Timer(100);
            CmdQueueTmr.Elapsed += CmdQueueTmr_Elapsed;
            CmdQueueTmr.AutoReset = false;

            task1 = new Task(new Action(CmdSendTask));
            task2 = new Task<List<Mtrf>>(new Func<List<Mtrf>>(SearchMtrf));
        }


        //Task for sequentially transmitting 
        private void CmdSendTask() {
            while (queue.Count != 0) {
                AnswerReceived.WaitOne(5000);
                AnswerReceived.Reset();
                SendData(queue.Dequeue());
            }
        }

        private void CmdQueueTmr_Elapsed(object sender, ElapsedEventArgs e) {
            if (task1.Status != TaskStatus.Running) {
                task1 = new Task(new Action(CmdSendTask));
                task1.Start();
            }
        }

        List<Mtrf> SearchMtrf() {
            string[] Ports = SerialPortStream.GetPortNames();
            List<Mtrf> connectedMtrfs = new List<Mtrf>();
            foreach (var portName in Ports) {
                OpenPort(portName);
                rxBuf = new Buf();
                SendCmd(0, NooMode.Service, 0, MtrfMode:NooCtr.ReadAnswer);
                AnswerReceived2.WaitOne(1000);
                ClosePort(portName);
                AnswerReceived2.Reset();
                if (rxBuf.AddrF != 0) {
                    connectedMtrfs.Add(new Mtrf(portName, rxBuf.AddrF));
                }
            }
            return connectedMtrfs;
        }

        public List<Mtrf> GetAvailableComPorts() {
            if (task2.Status != TaskStatus.Running) {
                task2 = new Task<List<Mtrf>>(new Func<List<Mtrf>>(SearchMtrf));
            }
            task2.Start();
            return task2.Result;
        }


        void DataReceivedHandler(object sender, SerialDataReceivedEventArgs args) {
            Stream s1 = serialPort;
            BinaryReader b1 = new BinaryReader(s1);
            rxBuf.LoadData(b1.ReadBytes(17));

            if (rxBuf.GetCrc == rxBuf.Crc) {
                if (NewDataReceived != null) {
                    NewDataReceived(this, EventArgs.Empty);
                }
                System.Threading.Thread.Sleep(25);
                AnswerReceived.Set();
                AnswerReceived2.Set();
            }
        }

        public int ClosePort(string pName) {
            if (serialPort.IsOpen) {
                serialPort.Close();
                return 0;
            } else {
                return -1;
            }
        }
        public int OpenPort(string pName) {
            if (pName != null) {
                if (!serialPort.IsOpen) {
                    serialPort.PortName = pName;
                    serialPort.Open();
                }
                return 0;
            } else {
                return -1;
            }
        }
        public int OpenPort(Mtrf mtrf)  {
            if (mtrf.MtrfAddr != 0) {
                if (!serialPort.IsOpen) {
                    serialPort.PortName = mtrf.ComPortName;
                    serialPort.Open();
                }
                return 0;
            } else {
                return -1;
            }
        }

        void AddCmdToQueue(Buf buf) {
            CmdQueueTmr.Stop();
            if (queue.Count != 0) {
                CmdQueueTmr.Interval = 100;
            } else {
                CmdQueueTmr.Interval = 1;
            }
            queue.Enqueue(buf);
            CmdQueueTmr.Start();
        }

        public int SendData(Buf data) {
            if (serialPort.IsOpen) {
                serialPort.Write(data.GetBufData(), 0, 17);
                txBuf = data;
                if (DataSent != null) {
                    DataSent(this, EventArgs.Empty);
                }
                return 1;
            } else {
                return 0;
            }
        }

        public string GetLogMsg(Buf buf) {
            string str1 = serialPort.PortName + ": " + DateTime.Now.ToString("HH:mm:ss") + " ";
            for (CmdByteIdx i = CmdByteIdx.St; i <= CmdByteIdx.Sp; i++) {
                str1 += (i.ToString() + ":" + buf[i].ToString()).PadLeft(8) + " \n";
            }
            return str1;
        }

        public void StoreTemperature(ref float temp_celsius) {
            int temp = 0;
            int tempData = (rxBuf.D1 << 8 | rxBuf.D0) & 0x0FFF;
            if ((tempData & 0x0800) != 0) {
                temp = -1 * (4096 - tempData);  //temp value is negative
            } else {
                temp = tempData & 0x07FF;
            }
            temp_celsius = (float)(temp / 10.0);
        }

        public float GetTemperature(int channel) {
            if (channel >= 0 && channel < 64) {
                return LastTempBuf[channel];
            } else {
                return 65535;
            }
        }

        #region Noo Cmd code
        //public void BindOn(int channel, int mode, bool bindOff = false) {
        //    Buf txBuf = new Buf();
        //    txBuf.St = 171;
        //    txBuf.Mode = mode;
        //    if (bindOff) { //Send Bind Off
        //        txBuf.Ctr = 4;
        //    } else {
        //        if (mode == NooMode.Tx || mode == NooMode.FTx) {   //Send Bind Cmd
        //            txBuf.Ctr = 0;
        //            txBuf.Cmd = NooCmd.Bind;
        //        } else {                        //Send Enable Bind at channel
        //            txBuf.Ctr = 3;
        //            txBuf.Cmd = 0;
        //        }
        //    }

        //    txBuf.Ch = channel;
        //    txBuf.Crc = txBuf.GetCrc;
        //    txBuf.Sp = 172;

        //    AnswerReceived.Set();
        //    //AddCmdToQueue(txBuf);
        //    SendData(txBuf);
        //}
        public void Unbind(int channel, int mode, int addrF = 0, bool unbindAll = false) {
            Buf txBuf = new Buf();
            txBuf.St = 171;
            txBuf.Mode = mode;
            if (!unbindAll) { //clear all MTRF64 memory
                if (mode == NooMode.Rx || mode == NooMode.FRx) {
                    txBuf.Ctr = 5;
                } else {
                    txBuf.Ctr = 0;
                }
            } else {
                txBuf.Ctr = 6;
                txBuf.D0 = 170;
                txBuf.D1 = 85;
                txBuf.D2 = 170;
                txBuf.D3 = 85;
            }
            if (addrF != 0) {
                txBuf.Ch = 0;
                txBuf.AddrF = addrF;
            } else {
                txBuf.Ch = channel;
                txBuf.AddrF = 0;
            }
            txBuf.Cmd = NooCmd.Unbind;
            txBuf.Crc = txBuf.GetCrc;
            txBuf.Sp = 172;

            AnswerReceived.Set();
            AddCmdToQueue(txBuf);
            SendData(txBuf);
        }

        public void SendCmd(int channel, int mode, int cmd, int addrF = 0,
            int fmt = 0, int d0 = 0, int d1 = 0, int d2 = 0, int d3 = 0,
            int MtrfMode = 0) {
            Buf txBuf = new Buf();
            txBuf.St = 171;
            txBuf.Mode = mode;
            txBuf.Fmt = fmt;
            txBuf.D0 = d0;
            txBuf.D1 = d1;
            txBuf.D2 = d2;
            txBuf.D3 = d3;
            txBuf.AddrF = addrF;
            txBuf.Ctr = MtrfMode;
            txBuf.Ch = channel;
            txBuf.Cmd = cmd;
            txBuf.Crc = txBuf.GetCrc;
            txBuf.Sp = 172;
            if (MtrfMode == NooCtr.SendCmd || MtrfMode == NooCtr.SendByAdr) {
                AddCmdToQueue(txBuf);
            } else {
                SendData(txBuf);
            }
        }
        #endregion
    }

}


public struct Mtrf {
    public Mtrf(string pName, int addr) {
        ComPortName = pName;
        MtrfAddr = addr;
    }
    public string ComPortName { get; }
    public int MtrfAddr { get; }
    public string Info {
        get {
            return String.Format("{0}, MTRF64: {1}", ComPortName, MtrfAddr);
        }
    }
}

public static class NooCtr {
    public const int SendCmd = 0;
    public const int SendBroadcastCmd = 1;
    public const int ReadAnswer = 2;
    public const int BindModeEnable = 3;
    public const int BindModeDisable = 4;
    public const int ClearChannel = 5;
    public const int ClearAllChannel = 6;
    public const int UnbindAdrFromChannel = 7;
    public const int SendByAdr = 8;
}
public static class NooMode {
    public const int Tx = 0;
    public const int Rx = 1;
    public const int FTx = 2;
    public const int FRx = 3;
    public const int Service = 4;
    public const int FirmwUpd = 5;
}
public static class NooDevType {
    public const int RemController = 0;
    public const int PowerUnit = 1;
    public const int PowerUnitF = 2;
    public const int Sensor = 3;
}

public static class NooCmd {
    public const int Off = 0;
    public const int BrightDown = 1;
    public const int On = 2;
    public const int BrightUp = 3;
    public const int Switch = 4;
    public const int BrightBack = 5;
    public const int SetBrightness = 6;
    public const int LoadPreset = 7;
    public const int SavePreset = 8;
    public const int Unbind = 9;
    public const int StopReg = 10;
    public const int BrightStepDown = 11;
    public const int BrightStepUp = 12;
    public const int BrightReg = 13;
    public const int Bind = 15;
    public const int RollColor = 16;
    public const int SwitchColor = 17;
    public const int SwitchMode = 18;
    public const int SpeedModeBack = 19;
    public const int BatteryLow = 20;
    public const int SensTempHumi = 21;
    public const int TemporaryOn = 25;
    public const int Modes = 26;
    public const int ReadState = 128;
    public const int WriteState = 129;
    public const int SendState = 130;
    public const int Service = 131;
    public const int ClearMemory = 132;
}


