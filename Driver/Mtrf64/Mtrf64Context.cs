using System;
using System.Collections.Concurrent;
using System.Timers;
using System.IO;
using RJCP.IO.Ports;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Driver.Mtrf64
{
    public enum NooFSettingType
    {
        Base = 16,
        DimmmerCorrection = 17,
        OnLvl = 18
    }

    public enum CmdSendStatus
    {
        Ok,
        PortClosed,
        Error
    }

    public class BufferEventArgs : EventArgs
    {
        private readonly Buf _buffer;
        public BufferEventArgs(Buf buffer)
        {
            _buffer = buffer;
        }
        public Buf Buffer => _buffer;
    }

    public class Mtrf64Context : IDisposable
    {
        private bool _isInitialezed = false;
        private readonly ConcurrentQueue<Buf> _transmitQueue = new ConcurrentQueue<Buf>();
        private readonly ConcurrentQueue<Buf> _receivedQueue = new ConcurrentQueue<Buf>();

        public event EventHandler<BufferEventArgs> DataReceived;
        public event EventHandler DataSent;
        private readonly SerialPortStream _serialPort;

        //private readonly Timer _cmdQueueTmr;
        private Task _seqCmdSend;
        private Task _seqReadReceived;
        private Task<List<MtrfModel>> _searchMtrfTask;

        private readonly ManualResetEvent _answerReceived = new ManualResetEvent(true);
        private readonly ManualResetEvent _answerReceived2 = new ManualResetEvent(false);

        private readonly CancellationTokenSource _txTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _rxTokenSource = new CancellationTokenSource();

        public Mtrf64Context()
        {
            _serialPort = new SerialPortStream
            {
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None
            };

            //_cmdQueueTmr = new Timer(100);
            //_cmdQueueTmr.Elapsed += CmdQueueTmr_Elapsed;
            //_cmdQueueTmr.AutoReset = false;

            _seqCmdSend = new Task(CmdSendTask, _txTokenSource.Token, TaskCreationOptions.LongRunning);
            _seqCmdSend.Start();
            _seqReadReceived = new Task(ReadReceivedTask, _rxTokenSource.Token, TaskCreationOptions.LongRunning);
            _seqReadReceived.Start();
            _searchMtrfTask = new Task<List<MtrfModel>>(SearchMtrf);
        }


        //Task for sequentially reading 
        private void ReadReceivedTask(object token)
        {
            var cancelToken = (CancellationToken) token;
            while (cancelToken.IsCancellationRequested == false)
            {
                if (_isInitialezed)
                {
                   if(_receivedQueue.TryDequeue(out var bufferedData)) DataReceived?.Invoke(this, new BufferEventArgs(bufferedData));
                }
            }
        }
        //Task for sequentially transmitting 
        private async void CmdSendTask(object token)
        {
            var cancelToken = (CancellationToken) token;
            while (cancelToken.IsCancellationRequested == false)
            {
                while (!_transmitQueue.IsEmpty && _isInitialezed)
                {
                    if (_transmitQueue.TryDequeue(out var bufferedData)) await SendData(bufferedData);
                    _answerReceived.WaitOne(5000);
                    _answerReceived.Reset();
                }
            }
        }

        //private void CmdQueueTmr_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    if (_seqCmdSend.Status != TaskStatus.Running)
        //    {
        //        _seqCmdSend = new Task(new Action(CmdSendTask));
        //        _seqCmdSend.Start();
        //    }
        //}

        private List<MtrfModel> SearchMtrf()
        {
            var ports = SerialPortStream.GetPortNames();
            var connected = new List<MtrfModel>();
            foreach (var portName in ports)
            {
                OpenPort(portName);
                SendCmd(0, NooMode.Service, 0, MtrfMode: NooCtr.ReadAnswer);
                //todo: timeout 50s is for debug purposes only, otherwise change it to 1s 
                _answerReceived2.WaitOne(1000);
                ClosePort(portName);
                if (_receivedQueue.TryDequeue(out var rxBuf) && rxBuf.Length == 17)
                {
                    if (rxBuf.AddrF != 0)
                    {
                        connected.Add(new MtrfModel(portName, rxBuf.AddrF));
                    }
                }
                _answerReceived2.Reset();
            }
            _isInitialezed = true;
            return connected;
        }

        public Task<List<MtrfModel>> GetAvailableComPorts()
        {
            if (_searchMtrfTask.Status != TaskStatus.Created)
            {
                _searchMtrfTask = new Task<List<MtrfModel>>(SearchMtrf);
            }
            _searchMtrfTask.Start();
            return _searchMtrfTask;
        }


        void DataReceivedHandler(object sender, SerialDataReceivedEventArgs args)
        {
            var rxBuf = new Buf();
            var b1 = new BinaryReader(_serialPort);

                rxBuf.LoadData(b1.ReadBytes(17));
                if (rxBuf.Length == 17)
                {
                    if (rxBuf.GetCrc == rxBuf.Crc)
                    {
                        _receivedQueue.Enqueue(rxBuf);

                        Thread.Sleep(25);
                        
                        _answerReceived.Set();
                        _answerReceived2.Set();
                    }
                }

        }

        public int ClosePort(string pName)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.DataReceived -= new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
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
                if (!_serialPort.IsOpen)
                {
                    _serialPort.PortName = pName;
                    _serialPort.Open();
                    _serialPort.DataReceived += new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
                }
                return 0;
            }
            return -1;
        }
        public int OpenPort(MtrfModel mtrf)
        {
            if (mtrf.MtrfAddr != 0)
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.PortName = mtrf.ComPortName;
                    _serialPort.Open();
                    _serialPort.DataReceived += new EventHandler<SerialDataReceivedEventArgs>(DataReceivedHandler);
                }
                return 0;
            }
            return -1;
        }

        void AddCmdToQueue(Buf buf)
        {
            //_cmdQueueTmr.Stop();
            //if (_transmitQueue.Count != 0)
            //{
            //    _cmdQueueTmr.Interval = 100;
            //}
            //else
            //{
            //    _cmdQueueTmr.Interval = 1;
            //}
            _transmitQueue.Enqueue(buf);
            //_cmdQueueTmr.Start();
        }

        public async Task<CmdSendStatus> SendData(Buf data)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.BreakState = false;
                await _serialPort.WriteAsync(data.GetBufData(), 0, 17);
                _serialPort.Flush();

                    DataSent?.Invoke(this, EventArgs.Empty);
                    return CmdSendStatus.Ok;
            }
            return CmdSendStatus.PortClosed;
        }

        public string GetLogMsg(Buf buf)
        {
            string str1 = _serialPort.PortName + ": " + DateTime.Now.ToString("HH:mm:ss") + " ";
            for (CmdByteIdx i = CmdByteIdx.St; i <= CmdByteIdx.Sp; i++)
            {
                str1 += (i.ToString() + ":" + buf[i].ToString()).PadLeft(8) + " \n";
            }
            return str1;
        }

        public double ParseTemperature(Buf rxBuf)
        {
            var temp = 0;
            var tempData = (rxBuf.D1 << 8 | rxBuf.D0) & 0x0FFF;
            if ((tempData & 0x0800) != 0)
            {
                temp = -1 * (4096 - tempData);  //temp value is negative
            }
            else
            {
                temp = tempData & 0x07FF;
            }
            return (temp / 10.0);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _serialPort.Close();
                    _serialPort.Dispose();
                    
                    _rxTokenSource.Cancel();
                    _txTokenSource.Cancel();

                    _answerReceived.Close();
                    _answerReceived2.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
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
    public string Info => $"{ComPortName}, MTRF64: {MtrfAddr}";
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


