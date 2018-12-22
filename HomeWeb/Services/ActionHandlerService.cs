using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeWeb.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Driver.Mtrf64;
using HomeWeb.Models;

namespace HomeWeb.Services {
    public class ActionHandlerService {
        private readonly Mtrf64Context _mtrf64Context;
        private readonly DevicesService devicesService;
        private readonly ActionLogService actionLogService;
        //private readonly IHubContext<FeedbackHub> hubContext;
        private readonly NotificationService _notificationService;
        public string test = "";


        public ActionHandlerService(Mtrf64Context mtrf64Context, DevicesService devicesService, ActionLogService actionLogService, NotificationService notificationService) {
            this.devicesService = devicesService;
            this.actionLogService = actionLogService;
            _mtrf64Context = mtrf64Context;
            _notificationService = notificationService;
            //this.hubContext = hubContext;
            mtrf64Context.DataReceived += Mtrf64Context_NewDataReceived;
        }
        ~ActionHandlerService() {
            System.Diagnostics.Debug.WriteLine("Action handler destroyed");
        }

        private async void Mtrf64Context_NewDataReceived(object sender, EventArgs e) {
            ParseIncomingData();
            var devKey = _mtrf64Context.RxBuf.Mode == NooMode.FTx ? _mtrf64Context.RxBuf.AddrF : _mtrf64Context.RxBuf.Ch;
            if (devicesService.Devices.ContainsKey(devKey))
            {
                await _notificationService.NotifyAll<RfDevice>(ActionType.UpdateDevice, devicesService.Devices[devKey]);
                //if (hubContext != null) {
                //    await hubContext.Clients.All.SendAsync("UpdateDevice", devicesService.Devices[devKey]);
                //}
            }
        }

        private void ParseIncomingData() {
            RfDevice device = null;
            var containsDevice = false;

            if (_mtrf64Context.RxBuf.AddrF != 0) {
                if (devicesService.Devices.ContainsKey(_mtrf64Context.RxBuf.AddrF)) {
                    device = devicesService.Devices[_mtrf64Context.RxBuf.AddrF];
                    containsDevice = true;
                }
            } else {
                if (devicesService.Devices.ContainsKey(_mtrf64Context.RxBuf.Ch)) {
                    device = devicesService.Devices[_mtrf64Context.RxBuf.Ch];
                    containsDevice = true;
                }
            }

            if (containsDevice) {
                switch (_mtrf64Context.RxBuf.Cmd) {
                    case NooCmd.Switch:
                        //redirect
                        if (device.Type == NooDevType.RemController && device.Redirect.Count != 0) {
                            foreach (var item in device.Redirect) {
                                RfDevice dev = devicesService.Devices[item];
                                dev.SetSwitch(_mtrf64Context);
                            }
                        }
                        //Device.Log.Add(new LogItem(DateTime.Now, Device.State));
                        device.Log.Add(new LogItem(DateTime.Now, NooCmd.Switch));
                        break;
                    case NooCmd.On:
                        if (_mtrf64Context.RxBuf.Mode == NooMode.Tx) {
                            if (device.Type == NooDevType.PowerUnit) {
                                device.State = 1;
                                device.Bright = _mtrf64Context.RxBuf.D0;
                                device.Log.Add(new PuLogItem(DateTime.Now, NooCmd.On, device.State, device.Bright));
                            }

                        } else if (_mtrf64Context.RxBuf.Mode == NooMode.Rx) {
                            device.Log.Add(new LogItem(DateTime.Now, NooCmd.On));
                        }
                        break;
                    case NooCmd.Off:
                        if (_mtrf64Context.RxBuf.Mode == NooMode.Tx) {
                            device.State = 0;
                            device.Log.Add(new PuLogItem(DateTime.Now, NooCmd.Off, device.State, device.Bright));
                        } else if (_mtrf64Context.RxBuf.Mode == NooMode.Rx) {
                            device.Log.Add(new LogItem(DateTime.Now, NooCmd.Off));
                        }
                        break;
                    case NooCmd.SetBrightness:
                        device.ReadSetBrightAnswer(_mtrf64Context);
                        break;
                    case NooCmd.Unbind:
                        //_mtrf64Context.Unbind(_mtrf64Context.rxBuf.Ch, _mtrf64Context.rxBuf.Mode);
                        break;
                    case NooCmd.SensTempHumi:
                        _mtrf64Context.StoreTemperature(ref _mtrf64Context.LastTempBuf[_mtrf64Context.RxBuf.Ch]);
                        if (actionLogService.ActionLog.ContainsKey(_mtrf64Context.RxBuf.Ch)) {
                            actionLogService.ActionLog[_mtrf64Context.RxBuf.Ch].Add(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, _mtrf64Context.LastTempBuf[_mtrf64Context.RxBuf.Ch]));
                        } else {
                            actionLogService.ActionLog.Add(_mtrf64Context.RxBuf.Ch, new List<ILogItem>());
                            actionLogService.ActionLog[_mtrf64Context.RxBuf.Ch].Add(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, _mtrf64Context.LastTempBuf[_mtrf64Context.RxBuf.Ch]));
                        }
                        break;
                    case NooCmd.TemporaryOn:
                        int DevKey = _mtrf64Context.RxBuf.Ch;
                        if (!actionLogService.ActionLog.ContainsKey(DevKey)) {
                            actionLogService.ActionLog.Add(DevKey, new List<ILogItem>());
                        }
                        int count = actionLogService.ActionLog[DevKey].Count;
                        if (count > 0) {
                            DateTime previous = actionLogService.ActionLog[DevKey][count - 1].CurrentTime;
                            if (DateTime.Now.Subtract(previous).Seconds > 4) {
                                actionLogService.ActionLog[DevKey].Add(new LogItem(DateTime.Now, _mtrf64Context.RxBuf.D0));
                            }
                        } else {
                            actionLogService.ActionLog[DevKey].Add(new LogItem(DateTime.Now, _mtrf64Context.RxBuf.D0));
                        }
                        break;

                    case NooCmd.SendState:
                        //if(dev1.rxBuf.D0 == 5) { //suf-1-300
                        switch (_mtrf64Context.RxBuf.Fmt) {
                            case 0: //state
                                device.ReadState(_mtrf64Context);
                                device.Log.Add(new PuLogItem(DateTime.Now, _mtrf64Context.RxBuf.Cmd, device.State, device.Bright));
                                break;
                            case 16: //settings
                                device.Settings = _mtrf64Context.RxBuf.D1 << 8 | _mtrf64Context.RxBuf.D0;
                                break;
                            case 17: //dimmer correction lvls
                                device.DimCorrLvlHi = _mtrf64Context.RxBuf.D0;
                                device.DimCorrLvlLow = _mtrf64Context.RxBuf.D1;
                                break;
                            case 18:
                                device.OnLvl = _mtrf64Context.RxBuf.D0;
                                break;
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
