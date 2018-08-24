using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.SignalR;
using WebAppRfc.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Timers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebAppRfc.RF;

namespace WebAppRfc
{
    public class Program
    {
        public static MTRF Mtrf64;
        public static MyDB<int, RfDevice> DevBase;
        public static MyDB<int, List<ILogItem>> ActionLog;
        public static List<string> Rooms;
        public static void Main(string[] args)
        {
            ActionLog = MyDB<int, List<ILogItem>>.OpenFile("log.json");
            DevBase = MyDB<int, RfDevice>.OpenFile("devices.json");
            Rooms = GetRooms();
            Mtrf64 = new MTRF();
            List <Mtrf> availableAdapters = Mtrf64.GetAvailableComPorts();
            if (availableAdapters.Count > 0) {
                Mtrf64.OpenPort(availableAdapters[0]);
                Mtrf64.NewDataReceived += Mtrf64_NewDataReceived;
            }
            CreateWebHostBuilder(args).Build().Run();
        }

        private async static void Mtrf64_NewDataReceived(object sender, EventArgs e) {
            ParseIncomingData();
            int devKey = Mtrf64.rxBuf.Mode == NooMode.FTx ? Mtrf64.rxBuf.AddrF : Mtrf64.rxBuf.Ch;
            if (DevBase.Data.ContainsKey(devKey)) {
                if (FeedbackHub.GlobalContext != null) {
                    await FeedbackHub.GlobalContext.Clients.All.SendAsync("UpdateDevView", new JsonResult(
                        DevBase.Data[devKey]
                    ));
                }
            }
        }
        private static void ParseIncomingData() {
            RfDevice Device = new RfDevice();
            bool ContainsDevice = false;
 
            if (Mtrf64.rxBuf.AddrF != 0) {
                if (DevBase.Data.ContainsKey(Mtrf64.rxBuf.AddrF)) {
                    Device = DevBase.Data[Mtrf64.rxBuf.AddrF];
                    ContainsDevice = true;
                }
            } else {
                if (DevBase.Data.ContainsKey(Mtrf64.rxBuf.Ch)) {
                    Device = DevBase.Data[Mtrf64.rxBuf.Ch];
                    ContainsDevice = true;
                }
            }

            if (ContainsDevice) {
                switch (Mtrf64.rxBuf.Cmd) {
                    case NooCmd.Switch:
                        //redirect
                        if (Device.Type == NooDevType.RemController && Device.Redirect.Count != 0) {
                            foreach (var item in Device.Redirect) {
                                RfDevice dev = DevBase.Data[item];
                                dev.SetSwitch(Mtrf64);
                            }
                        }
                        //Device.Log.Add(new LogItem(DateTime.Now, Device.State));
                        Device.Log.Add(new LogItem(DateTime.Now, NooCmd.Switch));
                        break;
                    case NooCmd.On:
                        if (Mtrf64.rxBuf.Mode == NooMode.Tx) {
                            if (Device.Type == NooDevType.PowerUnit) {
                                Device.State = 1;
                                Device.Bright = Mtrf64.rxBuf.D0;
                                Device.Log.Add(new PuLogItem(DateTime.Now, NooCmd.On, Device.State, Device.Bright));
                            }
                            
                        } else if(Mtrf64.rxBuf.Mode == NooMode.Rx) {
                            Device.Log.Add(new LogItem(DateTime.Now, NooCmd.On));
                        }
                        break;
                    case NooCmd.Off:
                        if (Mtrf64.rxBuf.Mode == NooMode.Tx) {
                            Device.State = 0;
                            Device.Log.Add(new PuLogItem(DateTime.Now, NooCmd.Off, Device.State, Device.Bright));
                        } else if(Mtrf64.rxBuf.Mode == NooMode.Rx) {
                            Device.Log.Add(new LogItem(DateTime.Now, NooCmd.Off));
                        }
                        break;
                    case NooCmd.SetBrightness:
                        Device.ReadSetBrightAnswer(Mtrf64);
                        break;
                    case NooCmd.Unbind:
                        //Mtrf64.Unbind(Mtrf64.rxBuf.Ch, Mtrf64.rxBuf.Mode);
                        break;
                    case NooCmd.SensTempHumi:
                        Mtrf64.StoreTemperature(ref Mtrf64.LastTempBuf[Mtrf64.rxBuf.Ch]);
                        if (ActionLog.Data.ContainsKey(Mtrf64.rxBuf.Ch)) {
                            ActionLog.Data[Mtrf64.rxBuf.Ch].Add(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, Mtrf64.LastTempBuf[Mtrf64.rxBuf.Ch]));
                        } else {
                            ActionLog.Data.Add(Mtrf64.rxBuf.Ch, new List<ILogItem>());
                            ActionLog.Data[Mtrf64.rxBuf.Ch].Add(new SensLogItem(DateTime.Now, NooCmd.SensTempHumi, Mtrf64.LastTempBuf[Mtrf64.rxBuf.Ch]));
                        }
                        break;
                    case NooCmd.TemporaryOn:
                        int DevKey = Mtrf64.rxBuf.Ch;
                        if (!ActionLog.Data.ContainsKey(DevKey)) {
                            ActionLog.Data.Add(DevKey, new List<ILogItem>());
                        }
                        int count = ActionLog.Data[DevKey].Count;
                        if (count > 0) {
                            DateTime previous = ActionLog.Data[DevKey][count - 1].CurrentTime;
                            if (DateTime.Now.Subtract(previous).Seconds > 4) {
                                ActionLog.Data[DevKey].Add(new LogItem(DateTime.Now, Mtrf64.rxBuf.D0));
                            }
                        } else {
                            ActionLog.Data[DevKey].Add(new LogItem(DateTime.Now, Mtrf64.rxBuf.D0));
                        }
                        break;

                    case NooCmd.SendState:
                        //if(dev1.rxBuf.D0 == 5) { //suf-1-300
                        switch (Mtrf64.rxBuf.Fmt) {
                            case 0: //state
                                Device.ReadState(Mtrf64);
                                Device.Log.Add(new PuLogItem(DateTime.Now, Mtrf64.rxBuf.Cmd, Device.State, Device.Bright));
                                break;
                            case 16: //settings
                                Device.Settings = Mtrf64.rxBuf.D1 << 8 | Mtrf64.rxBuf.D0;
                                break;
                            case 17: //dimmer correction lvls
                                Device.DimCorrLvlHi = Mtrf64.rxBuf.D0;
                                Device.DimCorrLvlLow = Mtrf64.rxBuf.D1;
                                break;
                            case 18:
                                Device.OnLvl = Mtrf64.rxBuf.D0;
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
        public static List<string> GetRooms() {
            List<string> rooms;
            using (StreamReader s1 = new StreamReader(new FileStream("rooms.json", FileMode.Open))) {
                string res = s1.ReadToEnd();
                
                try {
                    rooms=JsonConvert.DeserializeObject<List<string>>(res, new JsonSerializerSettings {
                        Formatting = Formatting.Indented
                    });
                } catch {
                    rooms = new List<string> { "All" };
                }
            }
            return rooms;
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
