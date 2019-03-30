using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Driver.Mtrf64;
using Home.Web.Models;
using Home.Web.Services;
using Microsoft.Extensions.Caching.Memory;


namespace Home.Web.Services
{
    public class BindingService
    {
        private readonly DevicesService _devicesService;
        private readonly Mtrf64Context _mtrf64Context;
        private readonly NotificationService _notificationService;
        private readonly IMemoryCache _memoryCache;

        public BindingService(DevicesService devicesService, Mtrf64Context mtrf64Context, NotificationService notificationService, IMemoryCache memoryCache)
        {
            _devicesService = devicesService;
            _mtrf64Context = mtrf64Context;
            _notificationService = notificationService;
            _memoryCache = memoryCache;
            mtrf64Context.DataReceived += Dev1_NewDataReceived;
            timer1.Elapsed += Tmr_Tick;
        }

        public int FindedChannel { get; private set; }
        int SelectedType;
        bool WaitingBindFlag = false;

        public Device Device { get; private set; }
        public int KeyToAdd { get; private set; }
        public bool AddingOk { get; private set; }
        public string Status { get; private set; }
        Timer timer1 = new Timer();

        private async void Dev1_NewDataReceived(object sender, EventArgs e)
        {
            if (WaitingBindFlag)
            {
                switch (SelectedType)
                {
                    case NooDevType.PowerUnit:
                        if (_mtrf64Context.RxBuf.Cmd == NooCmd.Bind && FindedChannel == _mtrf64Context.RxBuf.Ch &&
                            _mtrf64Context.RxBuf.Mode == NooMode.Tx)
                        {
                            Status = "Bind to TX device send!";
                            await _notificationService.NotifyAll<Device, string>(ActionType.BindReceived, Device,
                                Status);
                            //await hubContext.Clients.All.SendAsync("BindReceived", Device, Status);
                        }
                        break;
                    case NooDevType.PowerUnitF:
                        if (_mtrf64Context.RxBuf.Mode == NooMode.FTx && _mtrf64Context.RxBuf.Ctr == NooCtr.BindModeEnable)
                        {
                            WaitingBindFlag = false;
                            Device.Addr = _mtrf64Context.RxBuf.AddrF;
                            KeyToAdd = Device.Addr;
                            Device.Key = KeyToAdd;
                            Status = "Bind F-TX accepted";
                            await _notificationService.NotifyAll<Device, string>(ActionType.BindReceived, Device,
                                Status);
                            //await hubContext.Clients.All.SendAsync("BindReceived", Device, Status);
                        }
                        break;
                    case NooDevType.Sensor:
                        if (_mtrf64Context.RxBuf.Cmd == NooCmd.Bind && _mtrf64Context.RxBuf.Fmt == 1 &&
                            FindedChannel == _mtrf64Context.RxBuf.Ch && _mtrf64Context.RxBuf.Mode == NooMode.Rx)
                        {
                            WaitingBindFlag = false;
                            Device.ExtDevType = _mtrf64Context.RxBuf.D0;
                            KeyToAdd = FindedChannel;
                            Device.Key = KeyToAdd;
                            Status = "Bind from sensor accepted";
                            await _notificationService.NotifyAll<Device, string>(ActionType.BindReceived, Device,
                                Status);
                            //await hubContext.Clients.All.SendAsync("BindReceived", Device, Status);
                        }
                        break;
                    default:
                        if (_mtrf64Context.RxBuf.Cmd == NooCmd.Bind && FindedChannel == _mtrf64Context.RxBuf.Ch
                            && _mtrf64Context.RxBuf.Mode == 1)
                        {
                            WaitingBindFlag = false;
                            KeyToAdd = FindedChannel;
                            Device.Key = KeyToAdd;
                            Status = "Bind from RC accepted";
                            await _notificationService.NotifyAll<Device, string>(ActionType.BindReceived, Device,
                                Status);
                            //await hubContext.Clients.All.SendAsync("BindReceived", Device, Status);
                        }
                        break;
                }
            }
        }


        private void Tmr_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (WaitingBindFlag)
            {
                Status = "Device not added";
                WaitingBindFlag = false;
                AddingOk = false;
            }
        }

        public async Task<int> FindEmptyChannel(int mode)
        {
            var devices = await _devicesService.GetDeviceList();
            //Noo-F mode
            if (mode == NooDevType.PowerUnitF)
            {

                if (devices.Where(x => x.Type == NooDevType.PowerUnitF).Count() < 64) return 0;
                else return -1; //noo F memory is Full
            }
            else
            { //Noo
                for (var i = 0; i < 64; i++)
                {
                    if (devices.Any(r => r.Key == i))
                    {
                        continue;
                    }
                    else
                    {
                        return i;
                    }
                }
                return -1; //noo memory is Full
            }
        }

        public void CancelBind()
        {
            switch (SelectedType)
            {
                case NooDevType.PowerUnit:
                    _mtrf64Context.UnbindTx(FindedChannel);
                    break;
                case NooDevType.PowerUnitF:
                    _mtrf64Context.UnbindFTx(Device.Addr);
                    break;
                default:
                    _mtrf64Context.UnbindSingleRx(FindedChannel);
                    break;
            }
        }

        public void SendBind()
        {
            if (SelectedType == NooDevType.PowerUnitF)
            {
                _mtrf64Context.SendCmd(0, NooMode.FTx, NooCmd.Bind);
                Status = "Waiting...";
                WaitingBindFlag = true;
                timer1.Interval = 1000;
                timer1.Start();
            }
            else
            {
                _mtrf64Context.SendCmd(FindedChannel, NooMode.Tx, NooCmd.Bind);
                WaitingBindFlag = true;
                timer1.Interval = 25000;
                timer1.Start();
            }
        }

        public async Task SendAdd()
        {
            KeyToAdd = FindedChannel;
            Device.Key = KeyToAdd;
            try
            {
                await _devicesService.ImportDeviceList(new List<Device>() { Device });
                Status = "Device added";
                AddingOk = true;
            }
            catch (Exception e)
            {
                Status = "Device not added\n" + e.Message;
                AddingOk = false;
            }
            WaitingBindFlag = false;
            //DeviceHub.GlobalContext.Clients.All.SendAsync("AddNewResult", Device, Status);
        }

        public async Task RoomSelected(NewDevModel newDev)
        {
            SelectedType = newDev.DevType;
            _mtrf64Context.SendCmd(0, 0, 0, MtrfMode: NooCtr.BindModeDisable); //send disable bind if enabled

            FindedChannel = await FindEmptyChannel(SelectedType);    //find empty channel
            if (FindedChannel != -1)
            {
                Device = new Device
                {
                    Name = newDev.Name,
                    Type = SelectedType,
                    Channel = FindedChannel,
                    Room = newDev.Room
                };

                switch (SelectedType)
                {
                    case NooDevType.PowerUnit:
                        WaitingBindFlag = false;
                        Status = "Press Send Bind";
                        break;
                    case NooDevType.PowerUnitF:
                        WaitingBindFlag = false;
                        Status = "Press service button";
                        break;
                    default: //NooDevType.RemController or NooDevType.Sensor  
                        _mtrf64Context.SendCmd(FindedChannel, NooMode.Rx, 0, MtrfMode: NooCtr.BindModeEnable); //enable bind at finded channel
                        Status = "Press service button  on RC/sensor";
                        WaitingBindFlag = true;
                        timer1.Interval = 25000;
                        timer1.Start();
                        break;
                }
            }
        }
    }

}
