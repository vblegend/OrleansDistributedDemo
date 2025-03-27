using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using OrleansDistributedDemo.Interfaces;

namespace OrleansDistributedDemo.Client
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // Parse command line arguments to determine which gateway port to connect to
                int gatewayPort = 30000;
                if (args.Length > 0 && int.TryParse(args[0], out int port))
                {
                    gatewayPort = port;
                }

                Console.WriteLine($"Connecting to Orleans cluster on gateway port {gatewayPort}");

                var host = Host.CreateDefaultBuilder(args)
                    .UseOrleansClient(clientBuilder =>
                    {
                        clientBuilder .UseLocalhostClustering(gatewayPort: gatewayPort)
                            .Configure<ClusterOptions>(options =>
                            {
                                options.ClusterId = "orleans-demo-cluster";
                                options.ServiceId = "OrleansDistributedDemo";
                            })
                        ;
                    })
                    .Build();

                await host.StartAsync();

                Console.WriteLine("Client connected successfully to Orleans cluster.");
                Console.WriteLine("Enter grain key (or leave empty for 'counter'):");
                string grainKey = Console.ReadLine()?.Trim() ?? "counter";
                if (string.IsNullOrEmpty(grainKey)) grainKey = "counter";


                var client = host.Services.GetRequiredService<IClusterClient>();
                var counterGrain = client.GetGrain<ICounterGrain>(grainKey);
                
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("\nChoose an operation:");
                    Console.WriteLine("1) Increment counter");
                    Console.WriteLine("2) Get current counter value");
                    Console.WriteLine("3) Reset counter");
                    Console.WriteLine("4) Get host information");
                    Console.WriteLine("5) Exit");
                    Console.Write("\nEnter your choice (1-5): ");
                    
                    string? choice = Console.ReadLine()?.Trim() ?? string.Empty;
                    
                    switch (choice)
                    {
                        case "1":
                            Console.Write("Enter increment value: ");
                            if (long.TryParse(Console.ReadLine() ?? "1", out long increment))
                            {
                                long newValue = await counterGrain.IncrementAsync(increment);
                                Console.WriteLine($"Counter incremented. New value: {newValue}");
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Using default increment of 1.");
                                long newValue = await counterGrain.IncrementAsync(1);
                                Console.WriteLine($"Counter incremented. New value: {newValue}");
                            }
                            break;
                            
                        case "2":
                            long currentValue = await counterGrain.GetCountAsync();
                            Console.WriteLine($"Current counter value: {currentValue}");
                            break;
                            
                        case "3":
                            await counterGrain.ResetAsync();
                            Console.WriteLine("Counter has been reset to 0.");
                            break;
                            
                        case "4":
                            string hostInfo = await counterGrain.GetHostInfoAsync();
                            Console.WriteLine($"Grain is hosted on: {hostInfo}");
                            break;
                            
                        case "5":
                            exit = true;
                            break;
                            
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }

                await host.StopAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client failed with error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}