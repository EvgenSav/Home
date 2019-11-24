using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Models;
using Home.Web.Services;
using MongoDB.Bson;

namespace Home.Web.Domain
{
    public class ActionProcessor
    {
        private readonly Mtrf64Context _mtrf64Context;
        private readonly DevicesService _devicesService;
        private readonly NotificationService _notificationService;
        private readonly RequestService _requestService;
        public ActionProcessor(Mtrf64Context mtrf64Context, DevicesService devicesService, 
            NotificationService notificationService, RequestService requestService)
        {
            _mtrf64Context = mtrf64Context;
            _devicesService = devicesService;
            _notificationService = notificationService;
            _requestService = requestService;
        }

        private async Task<int> FindEmptyChannel(DeviceTypeEnum mode)
        {
            var devices = await _devicesService.GetDeviceList();
            //Noo-F mode
            if (mode == DeviceTypeEnum.PowerUnitF)
            {

                if (devices.Count(x => x.Type == DeviceTypeEnum.PowerUnitF) < 64) return 0;
                return -1; //noo F memory is Full
            }
            var memoryCells = Enumerable.Range(1, 64);
            var free = memoryCells.Except(devices.Where(r => r.Key <= 63).Select(r => r.Key)).ToList();
            return free.Any() ? free.FirstOrDefault() : -1;
        }
        public async Task Process(IRequest request)
        {
            if (request.Type == RequestTypeEnum.Bind)
            {
                var emptyChannel = await FindEmptyChannel(request.DeviceType);
                request.Execute(emptyChannel);
                var requestDbo = request.GetRequestDbo();
                await _requestService.Update(requestDbo);
                await _notificationService.NotifyAll(ActionType.RequestUpdated, requestDbo);
            }
            else
            {
                request.Execute(0);
                if (request.DeviceFk.HasValue && (request.DeviceType == DeviceTypeEnum.RemoteController ||
                    request.DeviceType == DeviceTypeEnum.Sensor))
                {
                    request.Complete(null);
                    var requestDbo = request.GetRequestDbo();
                    await _requestService.Update(requestDbo);
                    await _notificationService.NotifyAll(ActionType.RequestUpdated, requestDbo);
                    await _devicesService.DeleteDeviceAsync(request.DeviceFk.Value);
                    await _notificationService.NotifyAll(ActionType.DeviceDeleted, request.DeviceFk);
                }
            }
        }

        public async Task Complete(IRequest request, int subType, int? deviceKey = null)
        {
            request.Complete(deviceKey);
            if (request.Step == RequestStepEnum.Completed)
            {
                var requestDbo = request.GetRequestDbo();
                if (request.Type == RequestTypeEnum.Bind)
                {
                    var dev = new Device
                    {
                        Key = request.DeviceFk.Value,
                        Name = request.Name,
                        Type = request.DeviceType,
                        SubType = subType,
                        Channel = requestDbo.MetaData.Channel
                    };
                   
                    await _devicesService.AddDevice(dev);
                    await _notificationService.NotifyAll(ActionType.DeviceAdded, dev);
                }
                else
                {
                    await _devicesService.DeleteDeviceAsync(request.DeviceFk.Value);
                    await _notificationService.NotifyAll(ActionType.DeviceDeleted, request.DeviceFk.Value);
                }
                await _requestService.Update(requestDbo);
                await _notificationService.NotifyAll(ActionType.RequestUpdated, requestDbo);
            }
        }

        public async Task<IRequest> GetPendingBind(Buf rxBuf)
        {
            var requests = await _requestService.GetBindings();
            var pendingRequests = requests
                .Where(r => r.Step == RequestStepEnum.Pending && GetDevMode(r.DeviceType) == rxBuf.Mode)
                .ToList();
            if (pendingRequests.Any())
            {
                foreach (var request in pendingRequests)
                {
                    switch (request.DeviceType)
                    {
                        case DeviceTypeEnum.PowerUnit:
                            if (rxBuf.Cmd == NooCmd.Bind && request.MetaData?.Channel == rxBuf.Ch) { 
                                return Request.FromDbo(request, _mtrf64Context);
                            }
                            break;
                        case DeviceTypeEnum.PowerUnitF:
                            if (rxBuf.Ctr == NooCtr.BindModeEnable)
                            {
                                return Request.FromDbo(request, _mtrf64Context);
                            }
                            break;
                        case DeviceTypeEnum.Sensor:
                            if (rxBuf.Cmd == NooCmd.Bind &&  request.MetaData?.Channel == rxBuf.Ch && rxBuf.Fmt == 1)
                            {
                                /*Device.SubType = e.Buffer.D0;*/
                                return  Request.FromDbo(request, _mtrf64Context);
                            }
                            break;
                        case DeviceTypeEnum.RemoteController:
                            if (rxBuf.Cmd == NooCmd.Bind && request.MetaData?.Channel == rxBuf.Ch)
                            {
                                return Request.FromDbo(request, _mtrf64Context);
                            }
                            break;
                        default:
                            return null;
                    }
                }
            }
            return null;
        }

        private int GetDevMode(DeviceTypeEnum type)
        {
            switch (type)
            {
                case DeviceTypeEnum.RemoteController:
                    return NooMode.Rx;
                case DeviceTypeEnum.Sensor:
                    return NooMode.Rx;
                case DeviceTypeEnum.PowerUnit:
                    return NooMode.Tx;
                case DeviceTypeEnum.PowerUnitF:
                    return NooMode.FTx;
                default:
                    return 0;
            }
        }

    }
}
