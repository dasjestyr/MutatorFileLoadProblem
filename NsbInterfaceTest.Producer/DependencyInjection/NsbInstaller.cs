using System;
using System.Threading.Tasks;
using Autofac;
using DAS.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using Serilog;

namespace NsbInterfaceTest.Producer.DependencyInjection
{
    public static class NsbInstaller
    {
        public static void AddNServiceBus(this IServiceCollection services,
            IContainer container,
            IConfiguration config,
            IHostingEnvironment environment)
        {
            // ensure the bus starts
            var instance = StartWithRetry(() => GetConfiguration(container, config, environment));
            services.AddSingleton<IMessageSession>(instance);
        }

        private static IEndpointInstance StartWithRetry(Func<EndpointConfiguration> configFactory, int maxAttempts = 10)
        {
            var attempts = 0;
            while (true)
            {
                try
                {
                    var configuration = configFactory();
                    var endpoint = Endpoint.Start(configuration).Result;
                    Console.Out
                        .WriteLineAsync("Transport connected!")
                        .Wait();

                    return endpoint;
                }
                catch (Exception ex)
                {
                    if (attempts > maxAttempts) throw;
                    var delay = 100 * (int)Math.Pow(2, attempts++);
                    Console.Out
                        .WriteLineAsync($"Startup failed, retry attempt {attempts} reason: {ex.Message}.")
                        .Wait();

                    Task.Delay(delay).Wait();
                }
            }
        }

        private static EndpointConfiguration GetConfiguration(
            IContainer container,
            IConfiguration config,
            IHostingEnvironment environment)
        {
            var endpointConfiguration = new EndpointConfiguration("DAS.Template.Demo");

            // if running local, skip. Will automatically use license located at:
            // Windows: %LOCALAPPDATA%\ParticularSoftware\license.xml
            // Linux/Mac: ${XDG_DATA_HOME:-$HOME/.local/share}/ParticularSoftware/license.xml
            if (!environment.IsEnvironment("Local"))
                endpointConfiguration.License(config["NServiceBus:License"]);

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.EnableInstallers();

            // concurrency
            var globalConcurrency = int.Parse(config["NServiceBus:GlobalConcurrency"]);
            if (globalConcurrency != 0) // used for debugging. When 0, it will be auto-calculated based on available CPUs
                endpointConfiguration.LimitMessageProcessingConcurrencyTo(globalConcurrency);

            // transport
            var transportConnectionString = config["NServiceBus:ConnectionStrings:Transport"];
            Logger.Verbose($"Using transport connection string: {transportConnectionString}", null);
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(transportConnectionString);
            transport.UseConventionalRoutingTopology();

            // persistence
            var isProdOrDev = environment.IsProduction() ||
                              environment.IsDevelopment();
            if (isProdOrDev)
            {
                var storageConnectionString = config["NServiceBus:ConnectionStrings:Persistence"];
                var persistence = endpointConfiguration.UsePersistence<AzureStoragePersistence>();
                persistence.ConnectionString(storageConnectionString);
            }
            else
            {
                //endpointConfiguration.UsePersistence<LearningPersistence>(); // use for persisting between restarts
                endpointConfiguration.UsePersistence<InMemoryPersistence>(); // use when persisting between restarts doesnt matter
            }

            // DI - Autofac is only used here to bridge IServiceCollection to NSB
            //containerBuilder.Populate(services);
            endpointConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));

            // Crash the container to allow orchestrator to restart the container
            // otherwise the bus may die and we may never know.
            endpointConfiguration.DefineCriticalErrorAction(context =>
            {
                var message = $"NSB Crashed: {context.Error}. The bus Shutting down...";
                Logger.Fatal(message, null, context.Exception);
                Log.CloseAndFlush();
                Environment.FailFast(message, context.Exception);
                return Task.CompletedTask;
            });

            return endpointConfiguration;
        }
    }
}
