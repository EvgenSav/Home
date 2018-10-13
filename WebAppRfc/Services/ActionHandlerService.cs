using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RFController;
using WebAppRfc.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WebAppRfc.Services {
    public class ActionHandlerService {
        private readonly Mtrf64Context mtrf64Context;
        private readonly DevicesService devicesService;
        private readonly ActionLogService actionLogService;
        private readonly IHubContext<FeedbackHub> hubContext;
        public string test = "";


        public ActionHandlerService(Mtrf64Context mtrf64Context, DevicesService devicesService, ActionLogService actionLogService, IHubContext<FeedbackHub> hubContext) {
            this.devicesService = devicesService;
            this.actionLogService = actionLogService;
            this.mtrf64Context = mtrf64Context;
            this.hubContext = hubContext;
            mtrf64Context.DataReceived += Mtrf64Context_NewDataReceived;
        }
        ~ActionHandlerService() {
            System.Diagnostics.Debug.WriteLine("Action handler destroyed");
        }

        private async void Mtrf64Context_NewDataReceived(object sender, EventArgs e) {
            ParseIncomingData();
            int devKey = mtrf64Context.RxBuf.Mode == NooMode.FTx ? mtrf64Context.RxBuf.AddrF : mtrf64Context.RxBuf.Ch;
            if (devicesService.Devices.ContainsKey(devKey)) {
                if (hubContext != null) {
                    await hubContext.Clients.All.SendAsync("UpdateDevView", devicesService.Devices[devKey]);
                }
            }
        }

        private void ParseIncomingData() {
            RfDevice Device = null;
            bool ContainsDevice = false;

            if (mtrf64Context.RxBuf.AddrF != 0) {
                if (devicesService.Devices.ContainsKey(mtrf64Context.RxBuf.AddrF)) {
                    Device = devicesService.Devices[mtrf64Context.RxBuf.AddrF];
                    ContainsDevice = true;
                }
            } else {
                if (devicesService.Devices.ContainsKey(mtrf64Context.RxBuf.Ch)) {
                    Device = devicesService.Devices[mtrf64Context.RxBuf.Ch];
                    ContainsDevice = true;
                }
            }

            if (ContainsDevice) {
                switch (mtrf64Context.RxBuf.Cmd) {
                    case NooCmd.Switch:
                        //redirect
                        if (Device.Type == NooDevType.RemController && Device.Redirect.Count != 0) {
                            foreach (var item in Device.Redirect) {
                                RfDevice dev = devicesService.Devices[item];
                                dev.SetSwitch(mtrf64Context);
                            }
                        }
                        //Device.Log.Add(new LogItem(DateTime.Now, Device.State));
                        Device.Log.Add(new LogItem(DateTime.Now, NooCmd.Switch));
                        break;
                    case NooCmd.On:
                        if (mtrf64Context.RxBuf.Mode == NooMode.Tx) {
                            if (Device.Type == NooDevType.PowerUnit) {
                                Device.State = 1;
                                Device.Bright = mtrf64Context.RxBuf.D0;
                                Device.Log.Add(new PuLogItem(DateTime.Now, NooCmd.On, Device.State, Device.Bright));
                            }

                        } else if (mtrf64Context.RxBuf.Mode == NooMode.Rx) {
                            Device.Log.Add(new LogItem(DateTime.Now, NooCmd.On));
                        }
                        break;
                    case NooCmd.Off:
                        if (mtrf64Context.RxBuf.Mode == NooMode.Tx) {
                            Device.State = 0;
                            Device.Log.Add(new PuLogItem(DateTime.Now, NooCmd.Off, Device.State, Device.Bright));
                        } else if (mtrf64Context.RxBuf.Mode == NooMode.Rx) {
                            Device.Log.Add(new LogItem(DateTime.Now, NooCmd.Off));
                        }
                        break;
                    case NooCmd.SetBrightness:
                        Device.ReadSetBrightAnswer(mtrf64Context);
                        break;
                    case NooCmd.Unbind:
                        //mtrf64Context.Unbind(mtrf64Context.rxBuf.Ch, mtrf64Context.rxBuf.Mode);
                        break;
                    case NooCmd.SensTempHumi:
                        mtrf64Context.StoreTemperature(ref mtrf64Context.LastTempBuf[mtrf64Context.RxBuf.Ch]);
                        if (actionLogService.ActionLog.ContainsKey(mtrf64Context.RxBuf.Ch)) {
                            actionLogService.ActionLog[mtrf64Context.RxBuf.Ch].Add(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, mtrf64Context.LastTempBuf[mtrf64Context.RxBuf.Ch]));
                        } else {
                            actionLogService.ActionLog.Add(mtrf64Context.RxBuf.Ch, new List<ILogItem>());
                            actionLogService.ActionLog[mtrf64Context.RxBuf.Ch].Add(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, mtrf64Context.LastTempBuf[mtrf64Context.RxBuf.Ch]));
                        }
                        break;
                    case NooCmd.TemporaryOn:
                        int DevKey = mtrf64Context.RxBuf.Ch;
                        if (!actionLogService.ActionLog.ContainsKey(DevKey)) {
                            actionLogService.ActionLog.Add(DevKey, new List<ILogItem>());
                        }
                        int count = actionLogService.ActionLog[DevKey].Count;
                        if (count > 0) {
                            DateTime previous = actionLogService.ActionLog[DevKey][count - 1].CurrentTime;
                            if (DateTime.Now.Subtract(previous).Seconds > 4) {
                                actionLogService.ActionLog[DevKey].Add(new LogItem(DateTime.Now, mtrf64Context.RxBuf.D0));
                            }
                        } else {
                            actionLogService.ActionLog[DevKey].Add(new LogItem(DateTime.Now, mtrf64Context.RxBuf.D0));
                        }
                        break;

                    case NooCmd.SendState:
                        //if(dev1.rxBuf.D0 == 5) { //suf-1-300
                        switch (mtrf64Context.RxBuf.Fmt) {
                            case 0: //state
                                Device.ReadState(mtrf64Context);
                                Device.Log.Add(new PuLogItem(DateTime.Now, mtrf64Context.RxBuf.Cmd, Device.State, Device.Bright));
                                break;
                            case 16: //settings
                                Device.Settings = mtrf64Context.RxBuf.D1 << 8 | mtrf64Context.RxBuf.D0;
                                break;
                            case 17: //dimmer correction lvls
                                Device.DimCorrLvlHi = mtrf64Context.RxBuf.D0;
                                Device.DimCorrLvlLow = mtrf64Context.RxBuf.D1;
                                break;
                            case 18:
                                Device.OnLvl = mtrf64Context.RxBuf.D0;
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
