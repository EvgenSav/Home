using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Driver.Mtrf64;
using Home.Web.Models;

namespace Home.Web.Extensions
{
    public static class DeviceExtensions
    {

        public static void SetOn(this RfDevice device, Mtrf64Context mtrfDev)
        {
            if (device.Type == NooDevType.PowerUnitF)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.On, addrF: device.Addr, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == NooDevType.PowerUnit)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.On);
            }
        }
        public static void SetOff(this RfDevice device, Mtrf64Context mtrfDev)
        {
            if (device.Type == NooDevType.PowerUnitF)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.Off, addrF: device.Addr, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == NooDevType.PowerUnit)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.Off);
            }
        }
        public static void SetSwitch(this RfDevice device, Mtrf64Context mtrfDev)
        {
            if (device.Type == NooDevType.PowerUnitF)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.Switch, addrF: device.Addr, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == NooDevType.PowerUnit)
            {
                if (device.State != 0)
                {
                    mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.Off);
                }
                else
                {
                    mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.On);
                }
            }
        }
        public static void SetBright(this RfDevice device, Mtrf64Context mtrfDev, int brightLvl)
        {
            int devBrightLvl = 0;
            if (device.Type == NooDevType.PowerUnitF)
            {
                devBrightLvl = Round(((float)brightLvl / 100) * 255);
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.SetBrightness, addrF: device.Addr, d0: devBrightLvl, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == NooDevType.PowerUnit)
            {
                devBrightLvl = Round(28 + ((float)brightLvl / 100) * 128);
                mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.SetBrightness, fmt: 1, d0: devBrightLvl);
            }
        }
        public static void ReadSetBrightAnswer(this RfDevice device, Mtrf64Context mtrfDev)
        {
            if (device.Type == NooDevType.PowerUnit)
            {
                if (mtrfDev.RxBuf.D0 >= 28)
                {
                    device.Bright = Round((((float)mtrfDev.RxBuf.D0 - 28) / 128) * 100);
                    if (mtrfDev.RxBuf.D0 > 28)
                    {
                        device.State = 1;
                    }
                    else
                    {
                        device.Bright = 0;
                        device.State = 0;
                    }
                }
                else
                {
                    device.Bright = 0;
                    device.State = 0;
                }
            }
        }
        public static void ReadState(this RfDevice device, Mtrf64Context mtrfDev)
        {
            if (device.Type == NooDevType.PowerUnitF)
            {
                device.ExtDevType = mtrfDev.RxBuf.D0;
                device.FirmwareVer = mtrfDev.RxBuf.D1;
                device.State = mtrfDev.RxBuf.D2;
                device.Bright = Round(((float)mtrfDev.RxBuf.D3 / 255) * 100);
            }
        }
        public static void Unbind(this RfDevice device, Mtrf64Context mtrfDev)
        {
            switch (device.Type)
            {
                case NooDevType.PowerUnit:
                    mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.Unbind);
                    break;

                    //case NooDevType.PowerUnitF:
                    //    DialogResult step21 = MessageBox.Show("Delete device?", "Warning!", MessageBoxButtons.YesNo);
                    //    if (step21 == DialogResult.Yes) {
                    //        Mtrf64.SendCmd(0, NooMode.FTx, NooCmd.Service, addrF: devToRemove.Addr, d0: 1, MtrfMode: NooCtr.SendByAdr);
                    //        Mtrf64.SendCmd(0, NooMode.FTx, NooCmd.Unbind, addrF: devToRemove.Addr, MtrfMode: NooCtr.SendByAdr);
                    //        //delete controls of device in each room
                    //        foreach (string roomToRemove in roomsToRemove) {
                    //            RemoveControl(devKey, roomToRemove);
                    //        }
                    //        //Remove device from base
                    //        DevBase.Data.Remove(devKey);
                    //    }
                    //    break;
                    //case NooDevType.RemController:
                    //    DialogResult step31 = MessageBox.Show("Delete device?", "Warning!", MessageBoxButtons.YesNo);
                    //    if (step31 == DialogResult.Yes) {
                    //        Mtrf64.Unbind(devToRemove.Channel, NooMode.Rx);
                    //        //delete controls of device in each room
                    //        foreach (string roomToRemove in roomsToRemove) {
                    //            RemoveControl(devKey, roomToRemove);
                    //        }
                    //        //Remove device from base
                    //        DevBase.Data.Remove(devKey);
                    //    }
                    //    break;
                    //case NooDevType.Sensor:
                    //    DialogResult step41 = MessageBox.Show("Delete device?", "Warning!", MessageBoxButtons.YesNo);
                    //    if (step41 == DialogResult.Yes) {
                    //        Mtrf64.Unbind(devToRemove.Channel, NooMode.Rx);
                    //        //delete controls of device in each room
                    //        foreach (string roomToRemove in roomsToRemove) {
                    //            RemoveControl(devKey, roomToRemove);
                    //        }
                    //        //Remove device from base
                    //        DevBase.Data.Remove(devKey);
                    //    }
                    //    break;

            }
            
        }
        private static int Round(float val)
        {
            if ((val - (int)val) > 0.5) return (int)val + 1;
            else return (int)val;
        }
    }
}
