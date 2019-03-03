using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataStorage;
using Home.Driver.Mtrf64;
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

namespace Home.Web
{
    public class Startup
    {
        IServiceProvider serviceProvider;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void OnStart()
        {
            BsonClassMap.RegisterClassMap<RfDevice>(r =>
            {
                r.AutoMap();
                r.SetDiscriminatorIsRequired(true);
            });
        }


        public async void OnShutdown()
        {
            /*var devicesService = serviceProvider.GetService<DevicesService>();
            var actionLogService = serviceProvider.GetService<ActionLogService>();
            var homeService = serviceProvider.GetService<HomeService>();
            await devicesService.SaveToFile("devices.json");
            await actionLogService.SaveToFile("log.json");*/
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DevicesService>();
            services.AddSingleton<ActionLogService>();
            var mtrf = new Mtrf64Context();
            var connected = new List<MtrfModel>();
            connected = mtrf.GetAvailableComPorts().Result;
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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //// In production, the Angular files will be served from this directory
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "wwwroot";
            //});
            services.AddSignalR();
            serviceProvider = services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(OnShutdown);
            applicationLifetime.ApplicationStarted.Register(OnStart);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseWebpackDevMiddleware(
                    new WebpackDevMiddlewareOptions
                    {
                        HotModuleReplacement = true
                    }
                    );
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            //app.UseSpaStaticFiles();

            app.UseCookiePolicy();
            app.UseWebSockets();
            app.UseSignalR(routes =>
            {
                routes.MapHub<FeedbackHub>("/devicesHub");
            });
            app.UseMvc(routes =>
            {

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

            });
            //app.UseSpa(spa =>
            //{
            //    //spa.Options.SourcePath = "src";
            //    if (env.IsDevelopment())
            //    {
            //        spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
            //        //spa.UseAngularCliServer(npmScript: "start");
            //        //spa.Options.DefaultPage = "/index.html";
            //    }
            //});
        }
    }
}
