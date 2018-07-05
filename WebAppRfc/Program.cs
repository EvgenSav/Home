using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RFController;

namespace WebAppRfc
{
    public class Program
    {
        public static MTRF Mtrf64;
        public static MyDB<int, RfDevice> DevBase;

        public static void Main(string[] args)
        {
            DevBase = MyDB<int, RfDevice>.OpenFile("devices.json");
            Mtrf64 = new MTRF();
            List<Mtrf> availableAdapters = Mtrf64.GetAvailableComPorts();
            if (availableAdapters.Count > 0) {
                Mtrf64.OpenPort(availableAdapters[0]);
            }
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
