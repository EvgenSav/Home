using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Home.Db.Storage;
using Home.Driver.Mtrf64;
using Home.Web.Extensions;
using Home.Web.Models;


namespace Home.Web.Services
{
    public class DevicesService
    {
        private readonly Mtrf64Context mtrf64Context;
        private MyDb<int, RfDevice> DevicesBase { get; set; }
        public SortedDictionary<int, RfDevice> Devices => DevicesBase.Data;
        public async Task SaveToFile(string path) => await DevicesBase.SaveToFile(path);

        public DevicesService(Mtrf64Context mtrf64Context)
        {
            DevicesBase = MyDb<int, RfDevice>.OpenFile("devices.json");
            this.mtrf64Context = mtrf64Context;
        }

        public async Task Update()
        {
            await SaveToFile("devices.json");
        }
        public void GetNooFSettings(int devId, int settingType)
        {
            Devices[devId].GetNooFSettings(mtrf64Context, settingType);
        }
        public void SetNooFSettings(int devId, NooFSettingType settingType, int settings)
        {
            Devices[devId].SetNooFSettings(mtrf64Context, settingType, settings);
        }
        public void Switch(int devId)
        {
            Devices[devId].SetSwitch(mtrf64Context);
        }
    }
}
