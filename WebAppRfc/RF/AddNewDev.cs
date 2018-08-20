using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WebAppRfc.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using WebAppRfc.Models;


namespace RFController {
    public class AddNewDev {
        Hashtable ht = new Hashtable();
        
        public MyDB<int, RfDevice> Devices { get; private set; }    //device list
        MTRF Mtrf64;                    //MTRF driver
        List<string> Rooms;
        Timer timer1 = new Timer();

        public int FindedChannel { get; private set; }
        int SelectedType;
        bool WaitingBindFlag = false;

        public RfDevice Device { get; private set; }
        public int KeyToAdd { get; private set; }
        public bool AddingOk { get; private set; }
        public string Status { get; private set; }

        public AddNewDev(MyDB<int, RfDevice> devList, MTRF dev, List<string> rooms) {
            Devices = devList;
            Mtrf64 = dev;
            Rooms = rooms;
            Mtrf64.NewDataReceived += Dev1_NewDataReceived;

            timer1.Elapsed += Tmr_Tick;
            
        }

        private async void Dev1_NewDataReceived(object sender, EventArgs e) {
            if (WaitingBindFlag) {
                switch (SelectedType) {
                    case NooDevType.PowerUnitF:
                        if (Mtrf64.rxBuf.Mode == NooMode.FTx && Mtrf64.rxBuf.Ctr == NooCtr.BindModeEnable) {
                            WaitingBindFlag = false;
                            Device.Addr = Mtrf64.rxBuf.AddrF;
                            KeyToAdd = Device.Addr;
                            Device.Key = KeyToAdd;
                            Status = "Bind F-TX accepted";
                            await FeedbackHub.GlobalContext.Clients.All.SendAsync("BindReceived", Device);
                        }
                        break;
                    case NooDevType.Sensor:
                        if (Mtrf64.rxBuf.Cmd == NooCmd.Bind && Mtrf64.rxBuf.Fmt == 1 &&
                            FindedChannel == Mtrf64.rxBuf.Ch && Mtrf64.rxBuf.Mode == 1) {
                            WaitingBindFlag = false;
                            Device.ExtDevType = Mtrf64.rxBuf.D0;
                            KeyToAdd = FindedChannel;
                            Device.Key = KeyToAdd;
                            Status = "Bind from sensor accepted";
                            await FeedbackHub.GlobalContext.Clients.All.SendAsync("BindReceived", Device);
                        }
                        break;
                    default:
                        if (Mtrf64.rxBuf.Cmd == NooCmd.Bind && FindedChannel == Mtrf64.rxBuf.Ch
                            && Mtrf64.rxBuf.Mode == 1) {
                            WaitingBindFlag = false;
                            KeyToAdd = FindedChannel;
                            Device.Key = KeyToAdd;
                            Status = "Bind from RC accepted";
                            await FeedbackHub.GlobalContext.Clients.All.SendAsync("BindReceived", Device);
                        }
                        break;
                }
            }
        }


        private void Tmr_Tick(object sender, EventArgs e) {
            timer1.Stop();
            if (WaitingBindFlag) {
                Status = "Device not added";
                WaitingBindFlag = false;
                AddingOk = false;
            }
        }



        public int FindEmptyChannel(int mode) {
            int FAddrCount = 0;
            //Noo-F mode
            if (mode == NooDevType.PowerUnitF) {
                var res = Devices.Data.Where((x) => { return (x.Value.Type == NooDevType.PowerUnitF); });
                foreach (var item in res) {
                    FAddrCount++;
                    //MessageBox.Show(item.Key.ToString());
                }
                if (FAddrCount < 64) return 0;
                else return -1; //noo F memory is Full
            } else { //Noo
                for (int i = 0; i < 64; i++) {
                    if (Devices.Data.ContainsKey(i)) {
                        continue;
                    } else {
                        return i;
                    }
                }
                return -1; //noo memory is Full
            }
        }

        public void SendBind() {
            if (SelectedType == NooDevType.PowerUnitF) {
                Mtrf64.SendCmd(0, NooMode.FTx, NooCmd.Bind);
                Status = "Waiting...";
                WaitingBindFlag = true;
                timer1.Interval = 1000;
                timer1.Start();
            } else {
                Mtrf64.SendCmd(FindedChannel, NooMode.Tx, NooCmd.Bind);
                Status = "Bind send!";
                WaitingBindFlag = true;
                timer1.Interval = 25000;
                timer1.Start();
            }
        }





        public void SendAdd() {
            KeyToAdd = FindedChannel;
            Device.Key = KeyToAdd;
            try {
                Devices.Data.Add(FindedChannel, Device);
                Status = "Device added";
                AddingOk = true;
            } catch (Exception e){
                Status = "Device not added\n" + e.Message;
                AddingOk = false;
            }
            WaitingBindFlag = false;
            FeedbackHub.GlobalContext.Clients.All.SendAsync("AddNewResult", new JsonResult(Device), Status);
        }

        public void RoomSelected(NewDevModel newDev) {
            SelectedType = newDev.DevType;
            Mtrf64.SendCmd(0, 0, 0, MtrfMode: NooCtr.BindModeDisable); //send disable bind if enabled

            FindedChannel = FindEmptyChannel(SelectedType);    //find empty channel
            if (FindedChannel != -1) {
                Device = new RfDevice {
                    Name = newDev.Name,
                    Type = SelectedType,
                    Channel = FindedChannel,
                    Room = newDev.Room
                };

                switch (SelectedType) {
                    case NooDevType.PowerUnit:
                        WaitingBindFlag = false;
                        Status = "Press Send Bind";
                        break;
                    case NooDevType.PowerUnitF:
                        WaitingBindFlag = false;
                        Status = "Press service button";
                        break;
                    default: //NooDevType.RemController or NooDevType.Sensor  
                        Mtrf64.SendCmd(FindedChannel, NooMode.Rx, 0, MtrfMode: NooCtr.ClearChannel);   //first - clear
                        Mtrf64.SendCmd(FindedChannel, NooMode.Rx, 0, MtrfMode: NooCtr.BindModeEnable); //enable bind at finded chnannel

                        Status = "Waiting...";

                        WaitingBindFlag = true;
                        timer1.Interval = 25000;
                        timer1.Start();
                        break;
                }
            }
        }
    }
}
