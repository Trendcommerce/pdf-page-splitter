using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;
using System;
using System.IO;
using System.Linq;

namespace pdf_page_splitter.Logging
{
    public class LogDnaTextFormatter : ITextFormatter
    {
        private static readonly JsonValueFormatter ValueFormatter = new();
        private readonly string _application;
        private readonly string _environment;

        public LogDnaTextFormatter(string application, string environment)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            _ = logEvent ?? throw new ArgumentNullException(nameof(logEvent));
            _ = output ?? throw new ArgumentNullException(nameof(output));

            try
            {
                using (var buffer = new StringWriter())
                {
                    FormatContent(logEvent, buffer);
                    output.WriteLine(buffer.ToString());
                }
            }
#pragma warning disable CA1031 // ok here
            catch (Exception ex)
#pragma warning restore CA1031
            {
                LogNonFormattableEvent(logEvent, ex);
            }
        }

        private void FormatContent(LogEvent logEvent, TextWriter output)
        {
            _ = logEvent ?? throw new ArgumentNullException(nameof(logEvent));
            _ = output ?? throw new ArgumentNullException(nameof(output));

            output.Write($"{{\"timestamp\":\"{logEvent.Timestamp.UtcDateTime:O}\""); // https://docs.mezmo.com/docs/log-parsing#timestamp

            output.Write(",\"line\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);

            output.Write($",\"app\":\"{this._application}\"");
            output.Write($",\"level\":\"{Format(logEvent.Level, logEvent.Exception)}\""); // https://docs.mezmo.com/docs/log-parsing#log-level
            output.Write($",\"env\":\"{this._environment}\"");

            output.Write(",\"meta\":{");

            output.Write("\"template\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

            FormatProperties(logEvent, output);
            FormatTokens(logEvent, output);

            if (logEvent.Exception != null)
            {
                output.Write(",\"exception\":");
                JsonValueFormatter.WriteQuotedJsonString(ExceptionFormattingEnricher.FormatException(logEvent.Exception), output);
            }

            output.Write('}'); // meta

            output.Write('}');
        }

        private static void FormatTokens(LogEvent logEvent, TextWriter output)
        {
            var tokens = logEvent.MessageTemplate.Tokens.OfType<PropertyToken>().Where(pt => pt.Format != null);
            if (tokens.Any())
            {
                output.Write(",\"renderings\":[");

                var firstToken = true;

                foreach (var token in tokens)
                {
                    if (firstToken)
                        firstToken = false;
                    else
                        output.Write(",");

                    using (var renderedToken = new StringWriter())
                    {
                        token.Render(logEvent.Properties, renderedToken);
                        JsonValueFormatter.WriteQuotedJsonString(renderedToken.ToString(), output);
                    }
                }

                output.Write(']');
            }
        }

        private static void FormatProperties(LogEvent logEvent, TextWriter output)
        {
            if (logEvent.Properties.Count > 0)
            {
                output.Write(",");

                var firstProperty = true;

                foreach (var property in logEvent.Properties)
                {
                    if (firstProperty)
                        firstProperty = false;
                    else
                        output.Write(",");

                    JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                    output.Write(':');
                    ValueFormatter.Format(property.Value, output);
                }
            }
        }

        private static string Format(LogEventLevel level, Exception exception) => level switch
        {
            LogEventLevel.Information => "INFO",
            LogEventLevel.Warning => "WARN",
            _ => level.ToString().ToUpperInvariant(),
        };

        private static void LogNonFormattableEvent(LogEvent logEvent, Exception ex) =>
            SelfLog.WriteLine(
                "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
                logEvent.Timestamp.ToString("o"),
                logEvent.MessageTemplate.Text,
                ex);
    }
}
