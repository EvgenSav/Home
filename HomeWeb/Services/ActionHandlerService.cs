using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Domain;
using Microsoft.AspNetCore.Mvc;

using Home.Web.Extensions;
using Home.Web.Models;

namespace Home.Web.Services
{
    public class ActionHandlerService : IDisposable
    {
        private readonly Mtrf64Context _mtrf64Context;
        private readonly DevicesService _devicesService;
        private readonly ActionLogService _actionLogService;
        private readonly NotificationService _notificationService;
        private readonly RequestService _requestService;

        public ActionHandlerService(Mtrf64Context mtrf64Context, DevicesService devicesService, ActionLogService actionLogService, NotificationService notificationService,
            RequestService requestService)
        {
            _devicesService = devicesService;
            _actionLogService = actionLogService;
            _mtrf64Context = mtrf64Context;
            _notificationService = notificationService;
            _requestService = requestService;
            mtrf64Context.DataReceived += Mtrf64Context_NewDataReceived;
        }

        public void Dispose()
        {
            _mtrf64Context.DataReceived -= Mtrf64Context_NewDataReceived;
            _mtrf64Context.Dispose();
        }

        private async void Mtrf64Context_NewDataReceived(object sender, BufferEventArgs e)
        {
            var rxBuf = e.Buffer;
            ParseIncomingData(rxBuf);
            var devKey = rxBuf.Mode == NooMode.FTx ? rxBuf.AddrF : rxBuf.Ch;
            var device = await _devicesService.GetByIdAsync(devKey);
            if (device != null)
            {
                await _devicesService.Update(device);
                await _notificationService.NotifyAll(ActionType.DeviceUpdated, device);
            }
        }

        private async void ParseIncomingData(Buf rxBuf)
        {
            var device = await _devicesService.GetByIdAsync(rxBuf.Id);
            if (device != null)
            {
                switch (rxBuf.Cmd)
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
                        await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, null));
                        break;
                    case NooCmd.On:
                        if (rxBuf.Mode == NooMode.Tx)
                        {
                            if (device.Type == DeviceTypeEnum.PowerUnit)
                            {
                                device.State.LoadState = LoadStateEnum.On;
                                await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, device.State));
                            }

                        }
                        else if (rxBuf.Mode == NooMode.Rx)
                        {
                            await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, null));
                        }
                        break;
                    case NooCmd.Off:
                        if (rxBuf.Mode == NooMode.Tx)
                        {
                            device.State.LoadState = LoadStateEnum.Off;
                            await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, device.State));
                        }
                        else if (rxBuf.Mode == NooMode.Rx)
                        {
                            await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, null));
                        }
                        break;
                    case NooCmd.SetBrightness:
                        device.ReadSetBrightAnswer(rxBuf);
                        await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, null));
                        break;
                    case NooCmd.Unbind:
                        //_mtrf64Context.Unbind(_mtrf64Context.rxBuf.Ch, _mtrf64Context.rxBuf.Mode);
                        break;
                    case NooCmd.SensTempHumi:
                        var temperature = _mtrf64Context.ParseTemperature(rxBuf);
                        var measures = new Dictionary<string, double>();
                        measures.Add(SensorDataTypeEnum.Temperature.ToString(), temperature);
                        if ((rxBuf.D1 & 0x20) != 0)
                        {
                            measures.Add(SensorDataTypeEnum.Humidity.ToString(), rxBuf.D2);
                        }
                        device.State.MeasuredData = measures;
                        await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, device.State));
                        break;
                    case NooCmd.TemporaryOn:
                        var devLog = await _actionLogService.GetDeviceLog(device.Key);
                        var latest = devLog.OrderByDescending(r => r.TimeStamp).FirstOrDefault();
                        var measure = new Dictionary<string, double>();
                        if (latest != null)
                        {
                            measure.Add(SensorDataTypeEnum.TimeIntervalCount.ToString(), rxBuf.D0);
                            if (DateTime.Now.Subtract(latest.TimeStamp).Seconds > 4)
                            {
                                device.State.MeasuredData = measure;
                                await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, device.State));
                            }
                        }
                        else
                        {
                            measure.Add(SensorDataTypeEnum.TimeIntervalCount.ToString(), rxBuf.D0);
                            device.State.MeasuredData = measure;
                            await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, device.State));
                        }
                        break;

                    case NooCmd.SendState:
                        //received settings
                        if (Enum.IsDefined(typeof(NooFSettingType), rxBuf.Fmt))
                        {
                            device.Settings.SetReceivedSettings((NooFSettingType)rxBuf.Fmt, rxBuf.D0, rxBuf.D1);
                            await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, null));
                        }
                        //received state
                        if (rxBuf.Fmt == 0)
                        {

                            device.ReadState(rxBuf);
                            var state = rxBuf.GetDeviceState();
                            await _actionLogService.AddAsync(new LogItem(rxBuf, device.Type, state));
                        }
                        //bind received
                        if (rxBuf.Ctr == NooCtr.BindModeEnable)
                        {
                            var processorF = new ActionProcessor(_mtrf64Context, _devicesService, _notificationService, _requestService);
                            var requestF = await processorF.GetPendingBind(rxBuf);
                            if (requestF != null)
                            {
                                await processorF.Complete(requestF, rxBuf.SubType, rxBuf.AddrF);
                            }
                        }
                        break;
                    case NooCmd.WriteState:
                        if (Enum.IsDefined(typeof(NooFSettingType), rxBuf.Fmt))
                        {
                            device.Settings.SetReceivedSettings((NooFSettingType)rxBuf.Fmt, rxBuf.D0, rxBuf.D1);
                        }
                        break;
                    default:
                        break;
                }
            }
            switch (rxBuf.Cmd)
            {
                case NooCmd.Bind:
                    var processor = new ActionProcessor(_mtrf64Context, _devicesService, _notificationService, _requestService);
                    var request = await processor.GetPendingBind(rxBuf);
                    if (request != null) await processor.Complete(request, rxBuf.SubType);
                    break;
                case NooCmd.SendState:
                    //bind received
                    if (rxBuf.Ctr == NooCtr.BindModeEnable)
                    {
                        var processorF = new ActionProcessor(_mtrf64Context, _devicesService, _notificationService, _requestService);
                        var requestF = await processorF.GetPendingBind(rxBuf);
                        if (requestF != null)
                        {
                            await processorF.Complete(requestF, rxBuf.SubType, rxBuf.AddrF);
                        }
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
