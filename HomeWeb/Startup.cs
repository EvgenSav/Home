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

namespace Home.Web
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //First, get the incoming request
            var request = await FormatRequest(context.Request);

            //Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            //Create a new memory stream...
            using (var responseBody = new MemoryStream())
            {
                //...and use that for the temporary response body
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline, eventually returning to this class
                await _next(context);

                //Format the response from the server
                var response = await FormatResponse(context.Response);

                //TODO: Save log to chosen datastore

                //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            var body = request.Body;

            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableRewind();

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            //...Then we copy the entire request stream into the new buffer.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding...
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
            request.Body = body;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            //...and copy it into a string
            string text = await new StreamReader(response.Body).ReadToEndAsync();

            //We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
            return $"{response.StatusCode}: {text}";
        }
    }
    public class Startup
    {
        private IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
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

            services.AddMvc();

            /*//// In production, the Angular files will be served from this directory
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "wwwroot";
            //});*/
            services.AddSignalR();
            _serviceProvider = services.BuildServiceProvider();
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
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            //app.UseSpaStaticFiles();
            //app.UseMiddleware<RequestResponseLoggingMiddleware>();

            /*app.Use(async (context, next) =>
            {
                // Do loging
                // Do work that doesn't write to the Response.
                _logger.LogDebug($"{DateTime.Now.ToShortTimeString()} : {context.Request.QueryString.Value}");
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });*/

            app.UseCookiePolicy();
            app.UseWebSockets();
            app.UseSignalR(routes =>
            {
                routes.MapHub<DeviceHub>("/devicesHub");
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
