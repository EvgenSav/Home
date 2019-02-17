using System;
using System.Timers;
using System.IO;
using RJCP.IO.Ports;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Home.Driver.Mtrf64
{
    public enum NooFSettingType
    {
        Base = 16,
        DimmmerCorrection = 17,
        OnLvl = 18
    }
    public class Mtrf64Context
    {
        Queue<Buf> queue = new Queue<Buf>();
        public event EventHandler DataReceived;
        public event EventHandler DataSent;
        readonly SerialPortStream serialPort;
        public Buf RxBuf { get; private set; }
        public Buf TxBuf { get; private set; }
        public string ConnectedPortName {
            get {
                if (serialPort != null && serialPort.IsOpen)
                {
                    return serialPort.PortName;
                }
                else
                {
                    return "Not connected";
                }
            }
        }

        public float[] LastTempBuf { get; private set; }

        Timer CmdQueueTmr;
        Task SeqCmdSend;
        Task<List<MtrfModel>> SearchMtrfTask;

        System.Threading.ManualResetEvent AnswerReceived = new System.Threading.ManualResetEvent(true);
        System.Threading.ManualResetEvent AnswerReceived2 = new System.Threading.ManualResetEvent(false);

        public Mtrf64Context()
        {
            RxBuf = new Buf();
            TxBuf = new Buf();

            serialPort = new SerialPortStream
            {
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None
            };

            LastTempBuf = new float[64];
            for (int i = 0; i < LastTempBuf.Length; i++)
            {
                LastTempBuf[i] = 65535;
            }

            CmdQueueTmr = new Timer(100);
            CmdQueueTmr.Elapsed += CmdQueueTmr_Elapsed;
            CmdQueueTmr.AutoReset = false;

            SeqCmdSend = new Task(new Action(CmdSendTask));
            SearchMtrfTask = new Task<List<MtrfModel>>(new Func<List<MtrfModel>>(SearchMtrf));
        }


        //Task for sequentially transmitting 
        private async void CmdSendTask()
        {
            while (queue.Count != 0)
            {
                AnswerReceived.WaitOne(5000);
                AnswerReceived.Reset();
                await SendData(queue.Dequeue());
            }
        }

        private void CmdQueueTmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (SeqCmdSend.Status != TaskStatus.Running)
            {
                SeqCmdSend = new Task(new Action(CmdSendTask));
                SeqCmdSend.Start();
            }
        }

        List<MtrfModel> SearchMtrf()
        {
            string[] Ports = SerialPortStream.GetPortNames();
            List<MtrfModel> connectedMtrfs = new List<MtrfModel>();
            foreach (var portName in Ports)
            {
                OpenPort(portName);
                SendCmd(0, NooMode.Service, 0, MtrfMode: NooCtr.ReadAnswer);
                AnswerReceived2.WaitOne(1000);
                ClosePort(portName);
                if (RxBuf.Length == 17)
                {
                    if (RxBuf.AddrF != 0)
                    {
                        connectedMtrfs.Add(new MtrfModel(portName, RxBuf.AddrF));
                    }
                }
                AnswerReceived2.Reset();
            }
            return connectedMtrfs;
        }

        public Task<List<MtrfModel>> GetAvailableComPorts()
        {
            return Task.Run(() => { return SearchMtrf(); });
        }


        void DataReceivedHandler(object sender, SerialDataReceivedEventArgs args)
        {
            BinaryReader b1 = new BinaryReader(serialPort);
            RxBuf.LoadData(b1.ReadBytes(17));
            if (RxBuf.Length == 17)
            {
                if (RxBuf.GetCrc == RxBuf.Crc)
                {
                    if (DataReceived != null)
                    {
                        DataReceived(this, EventArgs.Empty);
                    }
                    System.Threading.Thread.Sleep(25);
                    AnswerReceived.Set();
                    AnswerReceived2.Set();
                }
            }
        }

        public int ClosePort(string pName)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.DataReceived -= new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
                return 0;
            }
            else
            {
                return -1;
            }
        }
        public int OpenPort(string pName)
        {
            if (pName != null)
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = pName;
                    serialPort.Open();
                    serialPort.DataReceived += new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        public int OpenPort(MtrfModel mtrf)
        {
            if (mtrf.MtrfAddr != 0)
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = mtrf.ComPortName;
                    serialPort.Open();
                    serialPort.DataReceived += new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }

        void AddCmdToQueue(Buf buf)
        {
            CmdQueueTmr.Stop();
            if (queue.Count != 0)
            {
                CmdQueueTmr.Interval = 100;
            }
            else
            {
                CmdQueueTmr.Interval = 1;
            }
            System.Diagnostics.Debug.WriteLine(
                $"{DateTime.Now.TimeOfDay}: Ch:{buf.Ch}, AddrF:{buf.AddrF}, Cmd:{buf.Cmd} Fmt:{buf.Fmt}, D0: {buf.D0}, D1:{buf.D1}, D2: {buf.D3}, D3: {buf.D3}");
            queue.Enqueue(buf);
            CmdQueueTmr.Start();
        }

        public async Task<int> SendData(Buf data)
        {
            if (serialPort.IsOpen)
            {
                serialPort.BreakState = false;
                await serialPort.WriteAsync(data.GetBufData(), 0, 17);
                serialPort.Flush();
                TxBuf = data;
                if (DataSent != null)
                {
                    DataSent(this, EventArgs.Empty);
                }
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public string GetLogMsg(Buf buf)
        {
            string str1 = serialPort.PortName + ": " + DateTime.Now.ToString("HH:mm:ss") + " ";
            for (CmdByteIdx i = CmdByteIdx.St; i <= CmdByteIdx.Sp; i++)
            {
                str1 += (i.ToString() + ":" + buf[i].ToString()).PadLeft(8) + " \n";
            }
            return str1;
        }

        public void StoreTemperature(ref float temp_celsius)
        {
            int temp = 0;
            int tempData = (RxBuf.D1 << 8 | RxBuf.D0) & 0x0FFF;
            if ((tempData & 0x0800) != 0)
            {
                temp = -1 * (4096 - tempData);  //temp value is negative
            }
            else
            {
                temp = tempData & 0x07FF;
            }
            temp_celsius = (float)(temp / 10.0);
        }

        public float GetTemperature(int channel)
        {
            if (channel >= 0 && channel < 64)
            {
                return LastTempBuf[channel];
            }
            else
            {
                return 65535;
            }
        }


        public void BindTx(int channel)
        {
            SendCmd(channel, NooMode.Tx, NooCmd.Bind, MtrfMode: NooCtr.SendCmd);
        }
        public void UnbindTx(int channel)
        {
            SendCmd(channel, NooMode.Tx, NooCmd.Unbind, MtrfMode: NooCtr.SendCmd);
        }

        public void BindFTx(int channel = 0)
        {
            SendCmd(channel, NooMode.FTx, NooCmd.Bind, MtrfMode: NooCtr.SendCmd);
        }
        public void UnbindFTx(int devFtxAddr, int channel = 0)
        {
            SendCmd(channel, NooMode.FTx, NooCmd.Service, addrF: devFtxAddr, d0: 1, MtrfMode: NooCtr.SendByAdr);
            SendCmd(channel, NooMode.FTx, NooCmd.Unbind, addrF: devFtxAddr, MtrfMode: NooCtr.SendByAdr);
        }

        public void BindRxOn(int channel)
        {
            SendCmd(channel, NooMode.Rx, cmd: 0, MtrfMode: NooCtr.BindModeEnable);
        }
        public void BindModeOff()
        {
            SendCmd(0, 0, 0, MtrfMode: NooCtr.BindModeDisable);
        }
        public void UnbindSingleRx(int channel)
        {
            SendCmd(channel, NooMode.Rx, cmd: 0, MtrfMode: NooCtr.ClearChannel);
        }
        public void UnbindAllRx()
        {
            SendCmd(0, NooMode.Rx, cmd: 0, MtrfMode: NooCtr.ClearAllChannel, d0: 170, d1: 85, d2: 170, d3: 85);
        }

        public void GetSettings(int nooFaddr, int fmt)
        {
            SendCmd(0, NooMode.FTx, NooCmd.ReadState, nooFaddr, fmt, MtrfMode: NooCtr.SendByAdr);
        }

        public void SetSettings(int nooFaddr, NooFSettingType settingType, int settings)
        {
            SendCmd(0, NooMode.FTx, NooCmd.WriteState, nooFaddr, (int)settingType, d0: settings & 0xFF, d1: settings >> 8, d2: 255, d3: 255, MtrfMode: NooCtr.SendByAdr);
        }

        public async void SendCmd(int channel, int mode, int cmd, int addrF = 0,
            int fmt = 0, int d0 = 0, int d1 = 0, int d2 = 0, int d3 = 0,
            int MtrfMode = 0)
        {
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
            if (MtrfMode == NooCtr.SendCmd || MtrfMode == NooCtr.SendByAdr)
            {
                AddCmdToQueue(txBuf);
            }
            else
            {
                await SendData(txBuf);
            }
        }
    }

}


public struct MtrfModel
{
    public MtrfModel(string pName, int addr)
    {
        ComPortName = pName;
        MtrfAddr = addr;
    }
    public string ComPortName { get; }
    public int MtrfAddr { get; }
    public string Info {
        get {
            return string.Format("{0}, MTRF64: {1}", ComPortName, MtrfAddr);
        }
    }
}
public static class NooCtr
{
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
public static class NooMode
{
    public const int Tx = 0;
    public const int Rx = 1;
    public const int FTx = 2;
    public const int FRx = 3;
    public const int Service = 4;
    public const int FirmwUpd = 5;
}
public struct NooData
{
    public NooData(int d0 = 0, int d1 = 0, int d2 = 0, int d3 = 0)
    {
        D0 = d0;
        D1 = d1;
        D2 = d2;
        D3 = d3;
    }
    public int D0 { get; set; }
    public int D1 { get; set; }
    public int D2 { get; set; }
    public int D3 { get; set; }
}
public static class NooDevType
{
    public const int RemController = 0;
    public const int PowerUnit = 1;
    public const int PowerUnitF = 2;
    public const int Sensor = 3;
}
public static class NooCmd
{
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


