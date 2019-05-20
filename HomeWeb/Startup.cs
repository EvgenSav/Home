using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataStorage;
using Home.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Home.Web.Hubs;
using Home.Web.Models;
using MongoDB.Bson.Serialization;
using Driver.Mtrf64;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
namespace Home.Web
{
    public class Startup
    {
        private IServiceProvider _serviceProvider;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void OnStart()
        {
            BsonClassMap.RegisterClassMap<DatabaseModel>(r =>
            {
                r.AutoMap();
                r.SetIsRootClass(true);
            });
            BsonClassMap.RegisterClassMap<Device>();
            BsonClassMap.RegisterClassMap<LogItem>();
        }

        Task WriteLog(string msg)
        {

            return Task.CompletedTask;
        }
        public void OnShutdown()
        {
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DevicesService>();
            services.AddSingleton<ActionLogService>();
            var mtrf = new Mtrf64Context();
            var connected = new List<MtrfModel>();
            connected = Task.Run(async () => await mtrf.GetAvailableComPorts()).Result;
            if (connected.Count > 0)
            {
                mtrf.OpenPort(connected[0]);
            }
            services.AddSingleton<Mtrf64Context>(mtrf);
            services.AddSingleton<NotificationService>();
            services.AddSingleton<ActionHandlerService>();
            services.AddSingleton<BindingService>();
            services.AddSingleton<HomeService>();
            services.AddSingleton<IMongoDbStorage, MongoDbStorageService>();
            services.AddMemoryCache();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;

            });            
            services.AddControllers();
            services.AddMvc().AddNewtonsoftJson();
            services.AddSignalR();
            _serviceProvider = services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(OnShutdown);
            applicationLifetime.ApplicationStarted.Register(OnStart);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCookiePolicy();
            app.UseWebSockets();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DeviceHub>("/devicesHub");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
