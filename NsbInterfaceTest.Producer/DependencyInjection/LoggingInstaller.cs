using System.Linq;
using DAS.Infrastructure.Ext;
using DAS.Infrastructure.Logging;
using DAS.Infrastructure.Logging.Loggly;
using Loggly.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace NsbInterfaceTest.Producer.DependencyInjection
{
    public static class LoggingInstaller
    {
        public static void AddCustomLogging(
            this IServiceCollection services,
            IConfiguration config,
            IHostingEnvironment environment)
        {
            services.Configure<LogglySettings>(options => config
                .GetSection("Logging:Loggly")
                .Bind(options));

            var settings = services.BuildServiceProvider().GetService<IOptions<LogglySettings>>().Value;

            var c = LogglyConfig.Instance;
            c.CustomerToken = settings.CustomerToken;
            c.ApplicationName = settings.ApplicationName;
            c.IsEnabled = settings.IsEnabled;
            c.ThrowExceptions = settings.ThrowExceptions;

            c.Transport.LogTransport = settings.Transport.LogTransport.FindEnum<LogTransport>();
            c.Transport.EndpointHostname = settings.Transport.EndpointHostname;
            c.Transport.EndpointPort = settings.Transport.EndpointPort;

            var simpleTags = settings.Tags.Simple.Select(t => new SimpleTag { Value = t });
            c.TagConfig.Tags.AddRange(simpleTags);

            var serilogger = BuildSerilogLogger(config, environment);

            Logger.SetLogger(new SerilogLogger(serilogger));
        }

        private static Serilog.ILogger BuildSerilogLogger(IConfiguration config, IHostingEnvironment env)
        {
            var logglyLevel = LogEventLevel.Debug;
            if (env.IsProduction())
                logglyLevel = LogEventLevel.Information;

            var loggerConfig = new LoggerConfiguration();
            loggerConfig
                .MinimumLevel.Verbose()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Debug(outputTemplate: SerilogLogger.DefaultTextLogEntryFormat)
                .WriteTo.Loggly(logglyLevel);

            return loggerConfig.CreateLogger();
        }
    }
}
