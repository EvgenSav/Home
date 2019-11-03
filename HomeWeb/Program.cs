using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace Home.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool isDevelopment = string.Equals(
                "Development",
                environmentName,
                StringComparison.OrdinalIgnoreCase);
            if (isDevelopment) return WebHost.CreateDefaultBuilder<Startup>(args);

            return WebHost.CreateDefaultBuilder<Startup>(args).UseKestrel(r => r.ListenAnyIP(80));
        }

    }
}
