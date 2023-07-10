using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace pdf_page_splitter.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceProvider, LoggerConfiguration> configureLogger)
        {
            _ = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            hostBuilder.UseSerilog(configureLogger, preserveStaticLogger: false);

            var loggerConfiguration = new SerilogLoggerConfiguration(builder => builder.UseSerilog(configureLogger, preserveStaticLogger: true));

            hostBuilder.ConfigureServices(services => services.AddSingleton<ILoggerConfiguration>(loggerConfiguration));

            return hostBuilder;
        }

        private sealed class SerilogLoggerConfiguration : ILoggerConfiguration
        {
            private readonly Func<IHostBuilder, IHostBuilder> configureLogging;

            public SerilogLoggerConfiguration(Func<IHostBuilder, IHostBuilder> configureLogging) => this.configureLogging = configureLogging;

            public IHostBuilder ConfigureLogging(IHostBuilder builder) => this.configureLogging.Invoke(builder);
        }
    }

    public interface ILoggerConfiguration
    {
        IHostBuilder ConfigureLogging(IHostBuilder builder);
    }
}
