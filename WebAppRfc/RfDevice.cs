using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFController {
    [Serializable]
    public class RfDevice {
        public int Channel { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public int Addr { get; set; }
        public int Bright { get; set; }
        public bool IsDimmable { get; set; }
        public int FirmwareVer { get; set; }
        public int ExtDevType { get; set; }
        public int Settings { get; set; }
        public int DimCorrLvlHi { get; set; }
        public int DimCorrLvlLow { get; set; }
        public int OnLvl { get; set; }
        public string Room { get; set; }
        [NonSerialized]
        public List<ILogItem> Log;
        public List<int> Redirect { get; } = new List<int>(16);
        private int key;
        public int Key {
            get {
                return key;
            }
            set {
                key = value;
                //if (!DevicesForm.ActionLog.Data.ContainsKey(key)) {
                //    DevicesForm.ActionLog.Data.Add(key, new List<ILogItem>());
                //}
                //Log = DevicesForm.ActionLog.Data[key];
            }
        }

        //[NonSerialized]
        //public SortedList<string, DevView> Views = new SortedList<string, DevView>();

        public int AddRedirect(int devid) {
            Redirect.Add(devid);
            return 0;
        }
        public void SetOn(MTRF mtrfDev) {
            if (Type == NooDevType.PowerUnitF) {
                mtrfDev.SendCmd(Channel, NooMode.FTx, NooCmd.On, addrF: Addr, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                mtrfDev.SendCmd(Channel, NooMode.Tx, NooCmd.On);
            }
        }
        public void SetOff(MTRF mtrfDev) {
            if (Type == NooDevType.PowerUnitF) {
                mtrfDev.SendCmd(Channel, NooMode.FTx, NooCmd.Off, addrF: Addr, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                mtrfDev.SendCmd(Channel, NooMode.Tx, NooCmd.Off);
            }
        }
        public void SetSwitch(MTRF mtrfDev) {
            if (Type == NooDevType.PowerUnitF) {
                mtrfDev.SendCmd(Channel, NooMode.FTx, NooCmd.Switch, addrF: Addr, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                if (State != 0) {
                    mtrfDev.SendCmd(Channel, NooMode.Tx, NooCmd.Off);
                } else {
                    mtrfDev.SendCmd(Channel, NooMode.Tx, NooCmd.On);
                }
            }
        }
        public int Round(float val) {
            if ((val - (int)val) > 0.5) return (int)val + 1;
            else return (int)val;
        }
        public void SetBright(MTRF mtrfDev, int brightLvl) {
            int devBrightLvl = 0;
            if (Type == NooDevType.PowerUnitF) {
                devBrightLvl = Round(((float)brightLvl / 100) * 255);
                mtrfDev.SendCmd(Channel, NooMode.FTx, NooCmd.SetBrightness, addrF: Addr, d0: devBrightLvl, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                devBrightLvl = Round(28 + ((float)brightLvl / 100) * 128);
                mtrfDev.SendCmd(Channel, NooMode.Tx, NooCmd.SetBrightness, fmt: 1, d0: devBrightLvl);
            }
        }
        public void ReadSetBrightAnswer(MTRF mtrfDev) {
            if (Type == NooDevType.PowerUnit) {
                if (mtrfDev.rxBuf.D0 >= 28) {
                    //Bright = DevicesForm.Round((((float)mtrfDev.rxBuf.D0 - 28) / 128) * 100);
                    if (mtrfDev.rxBuf.D0 > 28) {
                        State = 1;
                    } else {
                        Bright = 0;
                        State = 0;
                    }
                } else {
                    Bright = 0;
                    State = 0;
                }
            }
        }
        public void ReadState(MTRF mtrfDev) {
            if (Type == NooDevType.PowerUnitF) {
                ExtDevType = mtrfDev.rxBuf.D0;
                FirmwareVer = mtrfDev.rxBuf.D1;
                State = mtrfDev.rxBuf.D2;
                //Bright = DevicesForm.Round(((float)mtrfDev.rxBuf.D3 / 255) * 100);
            }
        }
        public void Unbind(MTRF mtrfDev) {

        }

        public string GetDevTypeName() {
            string res = "";
            switch (Type) {
                case NooDevType.RemController:
                    res = "Пульт";
                    break;
                case NooDevType.Sensor:
                    switch (ExtDevType) {
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
                    switch (ExtDevType) {
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
