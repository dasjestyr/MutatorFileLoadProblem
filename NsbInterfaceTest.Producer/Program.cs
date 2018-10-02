using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using NsbInterfaceTest.Producer.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NsbInterfaceTest.Producer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Out.WriteLine("Starting host...");

            // replacing default container with Autofac because NSB needs it to bridge with their DI
            // this may change in the future, hopefully.
            var autofacFactory = new AutofacServiceProviderFactory();

            var host = new HostBuilder()
                .UseEnvironment(Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Local")
                .UseServiceProviderFactory(autofacFactory)
                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {

                    var services = new ServiceCollection();
                    var environment = context.HostingEnvironment;
                    var config = context.Configuration;

                    services.AddOptions();
                    services.AddCustomLogging(config, environment);
                    builder.Populate(services);

                    builder.RegisterBuildCallback(container =>
                    {
                        services.AddNServiceBus(container, config, environment);
                    });
                })
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                    if (args != null) builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var environment = context.HostingEnvironment;
                    builder.SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("Configuration/appsettings.json", false)
                        .AddJsonFile($"Configuration/appsettings.{environment.EnvironmentName}.json", true)
                        .AddJsonFile($"Configuration/azurekeyvault.{environment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables();

                    builder.LoadConfigurationValues(environment);
                }).Build();

            host.RunAsync().GetAwaiter().GetResult();

            Console.Out.WriteLine("Host is shutting down...");
        }
    }
}
