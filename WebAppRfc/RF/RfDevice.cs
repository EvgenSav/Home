using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAppRfc;

namespace WebAppRfc.RF {
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
        //[NonSerialized]
        public List<ILogItem> Log;
        public List<int> Redirect { get; } = new List<int>(16);
        private int key;
        public int Key {
            get {
                return key;
            }
            set {
                key = value;
                if (!Program.ActionLog.Data.ContainsKey(key)) {
                    Program.ActionLog.Data.Add(key, new List<ILogItem>());
                }
                Log = Program.ActionLog.Data[key];
            }
        }

        //[NonSerialized]
        //public SortedList<string, DevView> Views = new SortedList<string, DevView>();

        public int AddRedirect(int devid) {
            Redirect.Add(devid);
            return 0;
        }
        public void SetOn(MTRF mtrf) {
            if (Type == NooDevType.PowerUnitF) {
                mtrf.SendCmd(Channel, NooMode.FTx, NooCmd.On, addrF: Addr, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                mtrf.SendCmd(Channel, NooMode.Tx, NooCmd.On);
            }
        }
        public void SetOff(MTRF mtrf) {
            if (Type == NooDevType.PowerUnitF) {
                mtrf.SendCmd(Channel, NooMode.FTx, NooCmd.Off, addrF: Addr, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                mtrf.SendCmd(Channel, NooMode.Tx, NooCmd.Off);
            }
        }
        public void SetSwitch(MTRF mtrf) {
            if (Type == NooDevType.PowerUnitF) {
                mtrf.SendCmd(Channel, NooMode.FTx, NooCmd.Switch, addrF: Addr, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                if (State != 0) {
                    mtrf.SendCmd(Channel, NooMode.Tx, NooCmd.Off);
                } else {
                    mtrf.SendCmd(Channel, NooMode.Tx, NooCmd.On);
                }
            }
        }
        public int Round(float val) {
            if ((val - (int)val) > 0.5) return (int)val + 1;
            else return (int)val;
        }
        public void SetBright(MTRF mtrf, int brightLvl) {
            int devBrightLvl = 0;
            if (Type == NooDevType.PowerUnitF) {
                devBrightLvl = Round(((float)brightLvl / 100) * 255);
                mtrf.SendCmd(Channel, NooMode.FTx, NooCmd.SetBrightness, addrF: Addr, d0: devBrightLvl, MtrfMode: NooCtr.SendByAdr);
            } else if (Type == NooDevType.PowerUnit) {
                devBrightLvl = Round(28 + ((float)brightLvl / 100) * 128);
                mtrf.SendCmd(Channel, NooMode.Tx, NooCmd.SetBrightness, fmt: 1, d0: devBrightLvl);
            }
        }
        public void ReadSetBrightAnswer(MTRF mtrf) {
            if (Type == NooDevType.PowerUnit) {
                if (mtrf.rxBuf.D0 >= 28) {
                    Bright = Round((((float)mtrf.rxBuf.D0 - 28) / 128) * 100);
                    if (mtrf.rxBuf.D0 > 28) {
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

        public void ReadState(MTRF mtrf) {
            if (Type == NooDevType.PowerUnitF) {
                ExtDevType = mtrf.rxBuf.D0;
                FirmwareVer = mtrf.rxBuf.D1;
                State = mtrf.rxBuf.D2;
                Bright = Round(((float)mtrf.rxBuf.D3 / 255) * 100);
            }
        }

        public void Unbind(MTRF mtrf) {
            switch (Type) {
                case NooDevType.PowerUnit:
                    mtrf.UnbindTx(Channel);
                    break;
                case NooDevType.PowerUnitF:
                    mtrf.UnbindFTx(Addr);
                    break;
                case NooDevType.Sensor:
                    mtrf.UnbindSingleRx(Channel);
                    break;
                case NooDevType.RemController:
                    mtrf.UnbindSingleRx(Channel);
                    break;
                default:
                    break;
            }
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
