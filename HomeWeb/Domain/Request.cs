using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Models;
using MongoDB.Bson;

namespace Home.Web.Domain
{
    public enum RequestTypeEnum
    {
        Bind,
        Unbind
    }

    public class MetaData
    {
        public int  Channel { get; set; }
        public int?  AddressF { get; set; }
    }
    public interface IRequest
    {
        ObjectId Id { get; }
        RequestTypeEnum Type { get; }
        int? DeviceFk { get; }
        DeviceTypeEnum DeviceType { get; }
        DateTime? Completed { get; }
        string Name { get; }
        RequestStepEnum Step { get; }
        void Execute(int channel);
        void Complete(int? deviceKey);
        RequestDbo GetRequestDbo();
    }
    public abstract class Request : IRequest
    {
        private protected readonly RequestDbo _request;
        private protected readonly Mtrf64Context _mtrf64Context;
        public ObjectId Id => _request.Id;
        public RequestTypeEnum Type => _request.Type;
        public DeviceTypeEnum DeviceType => _request.DeviceType;
        public int? DeviceFk => DeviceType != DeviceTypeEnum.PowerUnitF ? MetaData.Channel : MetaData.AddressF;
        public DateTime? Completed => _request.Completed;
        public string Name => _request.Name;
        public RequestStepEnum Step => _request.Step;
        protected MetaData MetaData => _request.MetaData;
        protected Request(RequestDbo request, Mtrf64Context mtrf64Context)
        {
            _request = request;
            _mtrf64Context = mtrf64Context;
        }
        

        public static IRequest FromDbo(RequestDbo requestDbo, Mtrf64Context context)
        {
            switch (requestDbo.Type)
            {
                case RequestTypeEnum.Bind:
                    return new BindRequest(context, requestDbo);
                case RequestTypeEnum.Unbind:
                    return  new UnbindRequest(context,requestDbo);
            }
            return null;
        }

        public abstract void Execute(int channel);
        public abstract void Complete(int? key);

        public RequestDbo GetRequestDbo()
        {
            return _request;
        }
    }

    public class BindRequest : Request
    {
        internal BindRequest(Mtrf64Context _mtrf64Context, RequestDbo request) : base(request, _mtrf64Context)
        {

        }

        public override void Execute(int channel)
        {
            if (channel == -1)
            {
                _request.Step = RequestStepEnum.Error;
                return;
            }
            _request.MetaData = new MetaData
            {
                Channel = channel
            };
            _request.Step = RequestStepEnum.Pending;
            _mtrf64Context.SendCmd(0, 0, 0, MtrfMode: NooCtr.BindModeDisable); //send disable bind if enabled
            switch (DeviceType)
            {
                case DeviceTypeEnum.PowerUnitF:
                    _mtrf64Context.SendCmd(MetaData.Channel, NooMode.FTx, NooCmd.Bind);
                    break;
                case DeviceTypeEnum.PowerUnit:
                    _mtrf64Context.SendCmd(MetaData.Channel, NooMode.Tx, NooCmd.Bind);
                    break;
                default:
                    _mtrf64Context.SendCmd(MetaData.Channel, NooMode.Rx, 0, MtrfMode: NooCtr.BindModeEnable);
                    break;
            }
        }

        public override void Complete(int? key)
        {
            if (DeviceType == DeviceTypeEnum.PowerUnitF)
            {
                if (!key.HasValue)
                {
                    _request.Step = RequestStepEnum.Error;
                    return;
                }
                _request.MetaData.AddressF = key;
            }

            _request.Step = RequestStepEnum.Completed;
            _request.Completed = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }
    }

    public class UnbindRequest : Request
    {
        public UnbindRequest(Mtrf64Context mtrf64Context, RequestDbo request) : base(request, mtrf64Context)
        {

        }

        public override void Complete(int? key)
        {
            throw new NotImplementedException();
        }

        public override void Execute(int channel)
        {
            switch (DeviceType)
            {
                case DeviceTypeEnum.PowerUnit:
                    if(DeviceFk.HasValue) _mtrf64Context.UnbindTx(DeviceFk.Value);
                    break;
                case DeviceTypeEnum.PowerUnitF:
                    if (DeviceFk.HasValue) _mtrf64Context.UnbindFTx(DeviceFk.Value);
                    break;
                default:
                    if (DeviceFk.HasValue) _mtrf64Context.UnbindSingleRx(DeviceFk.Value);
                    break;
            }
        }
    }
}
