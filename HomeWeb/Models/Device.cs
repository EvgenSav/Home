using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Extensions;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Home.Web.Models
{
    enum BaseSettings
    {
        SaveState = 0x01,
        Dimmable = 0x02,
        DefaultOn = 0x20
    }
    public enum DeviceTypeEnum
    {
        [DisplayName("Remote Controller")]
        RemoteController = 0,
        [DisplayName("Power Unit")]
        PowerUnit = 1,
        [DisplayName("Power UnitF")]
        PowerUnitF = 2,
        [DisplayName("Sensor")]
        Sensor = 3
    }

    public class SettingsOperation
    {
        public SettingsOperation()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public NooFSettingType SettingType { get; set; }
        public int Data { get; set; }
    }

    public class DeviceSettings
    {
        private readonly ConcurrentQueue<SettingsOperation> _operations = new ConcurrentQueue<SettingsOperation>();
        [JsonProperty]
        private int _settings;
        [JsonProperty]
        private int _dimCorrLvlHi;
        [JsonProperty]
        private int _dimCorrLvlLow;
        [JsonProperty]
        private int _onLvl;
        [JsonProperty(Required = Required.Default)]
        public bool IsSaveState {
            get => (_settings & (int)BaseSettings.SaveState) != 0;
            set => _operations.Enqueue(
                new SettingsOperation()
                {
                    SettingType = NooFSettingType.Base,
                    Data = value ? _settings | (int)BaseSettings.SaveState : _settings & ~(int)BaseSettings.SaveState
                });
        }
        [JsonProperty(Required = Required.Default)]
        public bool IsDimmable {
            get => ((_settings & (int)BaseSettings.Dimmable) != 0);
            set => _operations.Enqueue(
                 new SettingsOperation()
                 {
                     SettingType = NooFSettingType.Base,
                     Data = value ? _settings | (int)BaseSettings.Dimmable : _settings & ~(int)BaseSettings.Dimmable
                 });
        }
        [JsonProperty(Required = Required.Default)]
        public bool IsDefaultOn {
            get => ((_settings & (int)BaseSettings.DefaultOn) != 0);
            set => _operations.Enqueue(
                new SettingsOperation()
                {
                    SettingType = NooFSettingType.Base,
                    Data = value ? _settings | (int)BaseSettings.DefaultOn : _settings & ~(int)BaseSettings.DefaultOn
                });
        }
        [JsonProperty(Required = Required.Default)]
        public int DimCorrLvlHi {
            get => _dimCorrLvlHi;
            set => _operations.Enqueue(new SettingsOperation() { SettingType = NooFSettingType.DimmmerCorrection, Data = DimCorrLvlLow << 8 | value });
        }
        [JsonProperty(Required = Required.Default)]
        public int DimCorrLvlLow {
            get => _dimCorrLvlLow;
            set => _operations.Enqueue(new SettingsOperation() { SettingType = NooFSettingType.DimmmerCorrection, Data = value << 8 | DimCorrLvlHi });
        }
        [JsonProperty(Required = Required.Default)]
        public int OnLvl {
            get => _onLvl;
            set => _operations.Enqueue(new SettingsOperation() { SettingType = NooFSettingType.OnLvl, Data = value });
        }
        public bool GetOperation(out SettingsOperation operation) => _operations.TryDequeue(out operation);

        public void SetReceivedSettings(NooFSettingType type, int d0, int d1)
        {
            switch (type)
            {
                case NooFSettingType.Base:
                    _settings = d1 << 8 | d0;
                    break;
                case NooFSettingType.DimmmerCorrection:
                    _dimCorrLvlLow = d1;
                    _dimCorrLvlHi = d0;
                    break;
                case NooFSettingType.OnLvl:
                    _onLvl = d0;
                    break;
            }
        }
    }

    public class DatabaseModel : IDatabaseModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DatabaseModel()
        {
            Id = ObjectId.GenerateNewId();
        }
        public bool IsDeleted { get; set; }
    }
    public class Device : DatabaseModel
    {
        [JsonConstructor]
        public Device()
        {
        }
        public int Channel { get; set; }
        public DeviceTypeEnum Type { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public int Addr { get; set; }
        public int Bright { get; set; }
        public int FirmwareVer { get; set; }
        public int ExtDevType { get; set; }
        public string Room { get; set; }
        public DeviceSettings Settings { get; set; } = new DeviceSettings();
        public List<int> Redirect { get; set; } = new List<int>();
        public int Key { get; set; }

        public int AddRedirect(int devid)
        {
            Redirect.Add(devid);
            return 0;
        }

        /*public string GetDevTypeName()
        {
            string res = "";
            switch (Type)
            {
                case NooDevType.RemController:
                    res = "Пульт";
                    break;
                case NooDevType.Sensor:
                    switch (ExtDevType)
                    {
                        case 1:
                            res = "PT112";
                            break;
                        case 2:
                            res = "PT111";
                            break;
                        case 3:
                            res = "PM111";
                            break;
                        case 5:
                            res = "PM112";
                            break;
                    }
                    break;
                case NooDevType.PowerUnit:
                    res = "Сил. блок";
                    break;
                case NooDevType.PowerUnitF:
                    switch (ExtDevType)
                    {
                        case 0:
                            res = "MTRF-64";
                            break;
                        case 1:
                            res = "SLF-1-300";
                            break;
                        case 2:
                            res = "SRF-10-1000";
                            break;
                        case 3:
                            res = "SRF-1-3000";
                            break;
                        case 4:
                            res = "SRF-1-3000M";
                            break;
                        case 5:
                            res = "SUF-1-300";
                            break;
                        case 6:
                            res = "SRF-1-3000T";
                            break;
                    }
                    break;
            }
            return res;
        }*/
    }
}
