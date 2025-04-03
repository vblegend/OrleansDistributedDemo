using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;


namespace OrleansDistributedDemo.Silo
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Parse command-line arguments for silo configuration
                int siloPort = 11111;
                int gatewayPort = 30000;
                string siloName = "Silo1";

                if (args.Length >= 3)
                {
                    siloName = args[0];
                    if (!int.TryParse(args[1], out siloPort))
                        siloPort = 11111;
                    if (!int.TryParse(args[2], out gatewayPort))
                        gatewayPort = 30000;
                }

                Console.WriteLine($"Starting silo {siloName} with ports: Silo={siloPort}, Gateway={gatewayPort}");

                // Define the cluster configuration for this silo
                var host = Host.CreateDefaultBuilder(args)
                    .UseOrleans(siloBuilder =>
                    {
                      
                        siloBuilder
                        .AddMemoryGrainStorageAsDefault()
                            .UseLocalhostClustering(
                                    siloPort: siloPort,
                                    gatewayPort: gatewayPort,
                                    primarySiloEndpoint: new IPEndPoint(IPAddress.Loopback, 11111)
                                )
                            .ConfigureEndpoints(IPAddress.Loopback, siloPort, gatewayPort)
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "orleans-demo-cluster";
                                options.ServiceId = "OrleansDistributedDemo";
                            })
                            .Configure<SiloOptions>(options =>
                            {
                                options.SiloName = siloName;
                            })
                            .ConfigureLogging(logging =>
                            {
                                logging.AddConsole();
                                logging.SetMinimumLevel(LogLevel.Information);
                            })
                            .UseDashboard(options => 
                            { 
                                options.Port = siloPort + 1000; // Dashboard port is silo port + 1000
                                options.HostSelf = true;
                            }).AddRedisGrainStorage("Redis", options =>
                            {
                                options.ConfigurationOptions = ConfigurationOptions.Parse("127.0.0.1:6379,password=123456,db=12");
                                options.DeleteStateOnClear = false;
                            });
                    })
                    .ConfigureServices(services =>
                    {
                        services.Configure<ConsoleLifetimeOptions>(options =>
                        {
                            options.SuppressStatusMessages = true;
                        });
                    })
                    .UseConsoleLifetime()
                    
                    .Build();

                await host.StartAsync();

                Console.WriteLine($"Silo {siloName} started successfully. Press Enter to terminate...");
                Console.WriteLine($"Dashboard is available at http://localhost:{siloPort + 1000}");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silo terminated unexpectedly: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}