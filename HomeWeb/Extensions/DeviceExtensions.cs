using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Models;

namespace Home.Web.Extensions
{
    public static class DeviceExtensions
    {
        public static void SetOn(this Device device, Mtrf64Context mtrfDev)
        {
            if (device.Type == DeviceTypeEnum.PowerUnitF)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.On, addrF: device.Key, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == DeviceTypeEnum.PowerUnit)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.On);
            }
        }
        public static void SetOff(this Device device, Mtrf64Context mtrfDev)
        {
            if (device.Type == DeviceTypeEnum.PowerUnitF)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.Off, addrF: device.Key, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == DeviceTypeEnum.PowerUnit)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.Off);
            }
        }
        public static void SetSwitch(this Device device, Mtrf64Context mtrfDev)
        {
            if (device.Type == DeviceTypeEnum.PowerUnitF)
            {
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.Switch, addrF: device.Key, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == DeviceTypeEnum.PowerUnit)
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
        public static void SetBright(this Device device, Mtrf64Context mtrfDev, int brightLvl)
        {
            int devBrightLvl = 0;
            if (device.Type == DeviceTypeEnum.PowerUnitF)
            {
                devBrightLvl = Buf.Round(((float)brightLvl / 100) * 255);
                mtrfDev.SendCmd(device.Channel, NooMode.FTx, NooCmd.SetBrightness, addrF: device.Key, d0: devBrightLvl, MtrfMode: NooCtr.SendByAdr);
            }
            else if (device.Type == DeviceTypeEnum.PowerUnit)
            {
                devBrightLvl = Buf.Round(28 + ((float)brightLvl / 100) * 128);
                mtrfDev.SendCmd(device.Channel, NooMode.Tx, NooCmd.SetBrightness, fmt: 1, d0: devBrightLvl);
            }
        }
        public static void ReadSetBrightAnswer(this Device device, Buf rxBuf)
        {
            if (device.Type == DeviceTypeEnum.PowerUnit)
            {
                if (rxBuf.D0 >= 28)
                {
                    device.Bright = Buf.Round((((float)rxBuf.D0 - 28) / 128) * 100);
                    if (rxBuf.D0 > 28)
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
        public static void ReadState(this Device device, Buf rxBuf)
        {
            if (device.Type == DeviceTypeEnum.PowerUnitF)
            {
                device.ExtDevType = rxBuf.D0;
                device.FirmwareVer = rxBuf.D1;
                device.State = rxBuf.D2;
                device.Bright = Buf.Round(((float)rxBuf.D3 / 255) * 100);
            }
        }

        public static void GetNooFSettings(this Device device, Mtrf64Context mtrf, int settingType)
        {
            mtrf.GetSettings(device.Key, settingType);
        }

        public static void SetNooFSettings(this Device device, Mtrf64Context mtrf, NooFSettingType settingType, int settings)
        {
            mtrf.SetSettings(device.Key, settingType, settings);
        }
        public static void Unbind(this Device device, Mtrf64Context mtrfDev)
        {
            switch (device.Type)
            {
                case DeviceTypeEnum.PowerUnit:
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
    }
}
