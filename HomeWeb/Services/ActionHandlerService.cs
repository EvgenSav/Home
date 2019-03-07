using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Microsoft.AspNetCore.Mvc;

using Home.Web.Extensions;
using Home.Web.Models;

namespace Home.Web.Services
{
    public class ActionHandlerService
    {
        private readonly Mtrf64Context _mtrf64Context;
        private readonly DevicesService _devicesService;
        private readonly ActionLogService _actionLogService;
        //private readonly IHubContext<DeviceHub> hubContext;
        private readonly NotificationService _notificationService;
        public string test = "";


        public ActionHandlerService(Mtrf64Context mtrf64Context, DevicesService devicesService, ActionLogService actionLogService, NotificationService notificationService)
        {
            _devicesService = devicesService;
            _actionLogService = actionLogService;
            _mtrf64Context = mtrf64Context;
            _notificationService = notificationService;
            //this.hubContext = hubContext;
            mtrf64Context.DataReceived += Mtrf64Context_NewDataReceived;
        }
        ~ActionHandlerService()
        {
            System.Diagnostics.Debug.WriteLine("Action handler destroyed");
        }

        private async void Mtrf64Context_NewDataReceived(object sender, EventArgs e)
        {
            ParseIncomingData();
            var devKey = _mtrf64Context.RxBuf.Mode == NooMode.FTx ? _mtrf64Context.RxBuf.AddrF : _mtrf64Context.RxBuf.Ch;
            var device = await _devicesService.GetByIdAsync(devKey);
            if (device != null)
            {
                await _devicesService.Update(device);
                await _notificationService.NotifyAll<RfDevice>(ActionType.UpdateDevice, device);

                //if (hubContext != null) {
                //    await hubContext.Clients.All.SendAsync("UpdateDevice", _devicesService.Devices[devKey]);
                //}
            }
        }

        private async void ParseIncomingData()
        {
            var device = await _devicesService.GetByIdAsync(_mtrf64Context.RxBuf.Id);
            if (device != null)
            {
                switch (_mtrf64Context.RxBuf.Cmd)
                {
                    case NooCmd.Switch:
                        //redirect
                        if (device.Type == NooDevType.RemController && device.Redirect.Count != 0)
                        {
                            foreach (var devId in device.Redirect)
                            {
                                var dev = await _devicesService.GetByIdAsync(devId);
                                dev.SetSwitch(_mtrf64Context);
                            }
                        }
                        await _actionLogService.AddAsync(new LogItem(DateTime.Now, NooCmd.Switch) { DeviceFk = device.Key });
                        break;
                    case NooCmd.On:
                        if (_mtrf64Context.RxBuf.Mode == NooMode.Tx)
                        {
                            if (device.Type == NooDevType.PowerUnit)
                            {
                                device.State = 1;
                                device.Bright = _mtrf64Context.RxBuf.D0;
                                await _actionLogService.AddAsync(new PuLogItem(DateTime.Now, NooCmd.On, device.State, device.Bright));
                            }

                        }
                        else if (_mtrf64Context.RxBuf.Mode == NooMode.Rx)
                        {
                            await _actionLogService.AddAsync(new LogItem(DateTime.Now, NooCmd.On));
                        }
                        break;
                    case NooCmd.Off:
                        if (_mtrf64Context.RxBuf.Mode == NooMode.Tx)
                        {
                            device.State = 0;
                            await _actionLogService.AddAsync(new PuLogItem(DateTime.Now, NooCmd.Off, device.State, device.Bright));
                        }
                        else if (_mtrf64Context.RxBuf.Mode == NooMode.Rx)
                        {
                            await _actionLogService.AddAsync(new LogItem(DateTime.Now, NooCmd.Off));
                        }
                        break;
                    case NooCmd.SetBrightness:
                        device.ReadSetBrightAnswer(_mtrf64Context);
                        break;
                    case NooCmd.Unbind:
                        //_mtrf64Context.Unbind(_mtrf64Context.rxBuf.Ch, _mtrf64Context.rxBuf.Mode);
                        break;
                    case NooCmd.SensTempHumi:
                        var temperature = _mtrf64Context.ParseTemperature();
                        await _actionLogService.AddAsync(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, temperature));
                        break;
                    case NooCmd.TemporaryOn:
                        var devLog = await _actionLogService.GetDeviceLog(device.Key);
                        var latest = devLog.OrderByDescending(r => r.TimeStamp).FirstOrDefault();
                        if (latest != null)
                        {
                            if (DateTime.Now.Subtract(latest.TimeStamp).Seconds > 4)
                            {
                                await _actionLogService.AddAsync(new LogItem(DateTime.Now, _mtrf64Context.RxBuf.D0));
                            }
                        }
                        else
                        {
                            await _actionLogService.AddAsync(new LogItem(DateTime.Now, _mtrf64Context.RxBuf.D0));
                        }
                        break;

                    case NooCmd.SendState:
                        //if(dev1.rxBuf.D0 == 5) { //suf-1-300
                        if (Enum.IsDefined(typeof(NooFSettingType), _mtrf64Context.RxBuf.Fmt))
                        {
                            device.Settings.SetReceivedSettings((NooFSettingType)_mtrf64Context.RxBuf.Fmt, _mtrf64Context.RxBuf.D0, _mtrf64Context.RxBuf.D1);
                        }
                        switch (_mtrf64Context.RxBuf.Fmt)
                        {
                            case 0: //state
                                device.ReadState(_mtrf64Context);
                                await _actionLogService.AddAsync(new PuLogItem(DateTime.Now, _mtrf64Context.RxBuf.Cmd, device.State, device.Bright) { DeviceFk = device.Key });
                                break;
                                /*case 16: //settings
                                    /*device.Settings.Settings= _mtrf64Context.RxBuf.D1 << 8 | _mtrf64Context.RxBuf.D0;#1#
                                    device.Settings.SetReceivedSettings((NooFSettingType)_mtrf64Context.RxBuf.Fmt, _mtrf64Context.RxBuf.D0, _mtrf64Context.RxBuf.D1);
                                    break;
                                case 17: //dimmer correction lvls
                                    device.Settings.SetReceivedSettings((NooFSettingType)_mtrf64Context.RxBuf.Fmt, _mtrf64Context.RxBuf.D0, _mtrf64Context.RxBuf.D1);
                                    /*device.Settings.DimCorrLvlHi = _mtrf64Context.RxBuf.D0;
                                    device.Settings.DimCorrLvlLow = _mtrf64Context.RxBuf.D1;#1#
                                    break;
                                case 18:
                                    device.Settings.SetReceivedSettings((NooFSettingType)_mtrf64Context.RxBuf.Fmt, _mtrf64Context.RxBuf.D0, _mtrf64Context.RxBuf.D1);
                                    /*device.Settings.OnLvl = _mtrf64Context.RxBuf.D0;#1#
                                    break;*/
                        }
                        break;
                    case NooCmd.WriteState:
                        if (Enum.IsDefined(typeof(NooFSettingType), _mtrf64Context.RxBuf.Fmt))
                        {
                            device.Settings.SetReceivedSettings((NooFSettingType)_mtrf64Context.RxBuf.Fmt, _mtrf64Context.RxBuf.D0, _mtrf64Context.RxBuf.D1);
                        }
                        break;
                    default:
                        break;
                }
                //foreach (var item in Device.Views) {
                //    item.Value.UpdateView();
                //}
            }

        }
    }
}
