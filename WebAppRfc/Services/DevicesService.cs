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
using Driver.Mtrf64;
using Db;



namespace WebAppRfc.Services {
    public class DevicesService {
        private readonly Mtrf64Context mtrf64Context;

        private MyDb<int, RfDevice> DevicesBase { get; set; }
        public SortedDictionary<int, RfDevice> Devices => DevicesBase.Data;
        public void SaveToFile(string path) => DevicesBase.SaveToFile(path);

        public DevicesService(Mtrf64Context mtrf64Context) {
            DevicesBase = MyDb<int, RfDevice>.OpenFile("devices.json");
            this.mtrf64Context = mtrf64Context;
        }
    }
}
