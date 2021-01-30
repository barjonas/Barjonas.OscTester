using System;
using NLog.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.Net;

namespace Barjonas.OscTester
{
    class Program
    {
        static void Main(string[] args)
        {
            IHostBuilder? builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<Sys>();

                })
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.SetMinimumLevel(LogLevel.Trace);
                    logBuilder.AddNLog("NLog.config");

                }).UseConsoleLifetime();
            IHost host = builder.Build();
            using (IServiceScope? serviceScope = host.Services.CreateScope())
            {
                IServiceProvider? services = serviceScope.ServiceProvider;

                try
                {
                    Sys? service = services.GetRequiredService<Sys>();
                    service.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while creating service: {ex.Message}");
                }
            }
        }
    }
}
