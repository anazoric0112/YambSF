using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;

namespace YambPlatform
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class YambPlatform : StatelessService
    {
        public YambPlatform(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("CorsPolicy", policy =>
                            {
                                policy.SetIsOriginAllowed(origin =>
                                    {
                                        var allowedOrigins = new[] { "localhost", "mdcssql-prepes2", "CPC-anazo-BOJ2T" };
                                        var requestHost = new Uri(origin).Host;
                                        return allowedOrigins.Contains(requestHost);
                                    })
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials();
                            });
                        });

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
                        builder.Services.AddControllersWithViews();

                        var app = builder.Build();
                        if (!app.Environment.IsDevelopment())
                        {
                            app.UseExceptionHandler("/Home/Error");
                        }

                        app.UseCors("CorsPolicy");

                        app.UseStaticFiles();
                        app.UseRouting();
                        app.UseAuthorization();
                        app.MapControllerRoute(
                            name: "default",
                            pattern: "{controller=Home}/{action=Index}/{id?}"
                        );
                        
                        return app;

                    }))
            };
        }

        internal static Uri GetServiceName(ServiceContext context, string serviceName)
        {
            return new Uri($"{context.CodePackageActivationContext.ApplicationName}/{serviceName}");
        }
    }
}
