using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home.Driver.Mtrf64;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Home.Web.Models
{
    enum BaseSettings
    {
        SaveState = 0x01,
        Dimmable = 0x02,
        DefaultOn = 0x20
    }
    public class DeviceSettings
    {
        public int Settings { private get; set; }
        public bool IsSaveState => ((Settings & (int)BaseSettings.SaveState) != 0);
        public bool IsDimmable => ((Settings & (int)BaseSettings.Dimmable) != 0);
        public bool IsDefaultOn => ((Settings & (int)BaseSettings.DefaultOn) != 0);
        public int DimCorrLvlHi { get; set; }
        public int DimCorrLvlLow { get; set; }
        public int OnLvl { get; set; }
    }
    [Serializable]
    public class RfDevice
    {
        [JsonConstructor]
        public RfDevice()
        {
        }
        private int _key;
        public int Channel { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public int Addr { get; set; }
        public int Bright { get; set; }
        public int FirmwareVer { get; set; }
        public int ExtDevType { get; set; }
        public string Room { get; set; }
        public DeviceSettings Settings { get; set; } = new DeviceSettings();
        //public List<ILogItem> Log { get; set; } = new List<ILogItem>();
        public List<int> Redirect { get; set; } = new List<int>(16);
        public int Key {
            get => _key;
            set => _key = value;
        }

        public int AddRedirect(int devid)
        {
            Redirect.Add(devid);
            return 0;
        }




        public string GetDevTypeName()
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
        }
    }
}
