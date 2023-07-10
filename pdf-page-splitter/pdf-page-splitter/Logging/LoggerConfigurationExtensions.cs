using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Net;

namespace pdf_page_splitter.Logging
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration LogDNA(
            this LoggerSinkConfiguration sinkConfiguration,
            string apiKey,
            string appName = null,
            string commaSeparatedTags = null,
            string ingestUrl = "https://logs.mezmo.com/logs/ingest",
            string hostname = null,
            int? batchPostingLimit = null,
            int? queueLimit = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            _ = sinkConfiguration ?? throw new ArgumentNullException(nameof(sinkConfiguration));
            _ = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _ = ingestUrl ?? throw new ArgumentNullException(nameof(ingestUrl));

            ingestUrl += $"{(ingestUrl.Contains("?", StringComparison.OrdinalIgnoreCase) ? "&" : "?")}hostname={hostname ?? Dns.GetHostName().ToLowerInvariant()}";

            if (commaSeparatedTags != null)
            {
                ingestUrl += $"&tags={Uri.EscapeDataString(commaSeparatedTags)}";
            }

            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return sinkConfiguration.Http(
                ingestUrl,
                batchPostingLimit: batchPostingLimit ?? 50,
                queueLimit: queueLimit ?? 100,
                period: period ?? TimeSpan.FromSeconds(15),
                textFormatter: new LogDnaTextFormatter(appName?.ToLowerInvariant() ?? "unknown", environmentName?.ToLowerInvariant() ?? "unknown"),
                batchFormatter: new LogDnaBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new LogDnaHttpClient(apiKey));
        }
    }
}
