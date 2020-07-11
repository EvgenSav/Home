using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DataStorage;
using Home.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Home.Web.Hubs;
using Home.Web.Models;
using MongoDB.Bson.Serialization;
using Driver.Mtrf64;
using Home.Web.Domain;
using Home.Web.Domain.Automation;
using Home.Web.Domain.Automation.Condition;
using Home.Web.Serialization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json.Serialization;

namespace Home.Web
{
    public class Startup
    {
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
            BsonClassMap.RegisterClassMap<Device>().SetIgnoreExtraElements(true);
            BsonClassMap.RegisterClassMap<LogItem>().SetIgnoreExtraElements(true);
            BsonClassMap.RegisterClassMap<Condition>().SetIgnoreExtraElements(true);
            BsonClassMap.RegisterClassMap<DeviceCmdCondition>().SetIgnoreExtraElements(true);
            BsonClassMap.RegisterClassMap<DeviceStateCondition>().SetIgnoreExtraElements(true);
            //convention: ignore extra elements (that exists in document, but doesn't exist in document's model)
            //BsonSerializer.RegisterSerializer(typeof(IConditionItem), new ConditionItemSerializer());
            ConventionRegistry.Register("IgnoreExtraElements", new ConventionPack { new IgnoreExtraElementsConvention(true) }, type => true);
        }
        public void OnShutdown(object serviceProvider)
        {
            var provider = serviceProvider as IServiceProvider;
            provider.GetService<ActionHandlerService>()?.Dispose();
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //while (!Debugger.IsAttached)
            //{
            //    Thread.Sleep(100);
            //}
            services.AddTransient<DevicesService>();
            services.AddTransient<ActionLogService>();
            var mtrf = new Mtrf64Context();
            var connected = Task.Run(async () => await mtrf.GetAvailableComPorts()).Result;
            if (connected.Count > 0)
            {
                mtrf.OpenPort(connected[0]);
            }
            services.AddSingleton(mtrf);
            services.AddTransient<NotificationService>();
            services.AddSingleton<ActionHandlerService>();
            services.AddTransient<RequestService>();
            services.AddTransient<HomeService>();
            services.AddTransient<IAutomationService, AutomationService>();
            services.AddSingleton<IMongoDbStorage, MongoDbStorageService>();
            services.AddSingleton<IAutomationProcessorService, AutomationProcessorService>();
            services.AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromMinutes(5));
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;

            });
            services.Configure<JsonOptions>(config =>
            {
                config.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
                config.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });
            services.AddControllers().AddNewtonsoftJson(config => config.SerializerSettings.ContractResolver = new DefaultContractResolver());
            //services.AddRazorPages();
            services.AddSignalR().AddNewtonsoftJsonProtocol(config => config.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(OnShutdown, app.ApplicationServices);
            applicationLifetime.ApplicationStarted.Register(OnStart);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions { HotModuleReplacement = true });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.ApplicationServices.GetService<ActionHandlerService>();

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
