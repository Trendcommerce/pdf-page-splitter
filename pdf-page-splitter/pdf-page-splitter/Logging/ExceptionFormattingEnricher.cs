using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Text;

namespace pdf_page_splitter.Logging
{
    public class ExceptionFormattingEnricher : ILogEventEnricher
    {
        public const string EnrichedExceptionPropertyName = "_detailedException";
        private const int TabSize = 4;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            _ = logEvent ?? throw new ArgumentNullException(nameof(logEvent));
            _ = propertyFactory ?? throw new ArgumentNullException(nameof(propertyFactory));

            if (logEvent.Exception != null)
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(EnrichedExceptionPropertyName, FormatException(logEvent.Exception)));
        }

        public static string FormatException(Exception arg)
        {
            var builder = new StringBuilder();
            FormatException(arg, builder, 1);
            return builder.ToString();
        }

        private static void FormatException(Exception exception, StringBuilder logs, int tabs)
        {
            if (exception == null)
                return;

            var indent = new string(' ', (tabs + 1) * TabSize);

            logs.AppendLine()
                .AppendLine($"{indent}{exception.GetType()?.Name}:")
                .AppendLine($"{indent}Message    : '{exception.Message}");

            if (exception.Data.Count > 0)
            {
                logs.Append($"{indent}Data       : ");
                FormatData(exception.Data, logs, tabs);
                logs.AppendLine();
            }

            var sanitizedStacktrace = exception.StackTrace?.Replace(
                Environment.NewLine,
                Environment.NewLine + indent + new string(' ', TabSize),
                StringComparison.OrdinalIgnoreCase);

            logs.AppendLine($"{indent}StackTrace : '{Environment.NewLine}{indent}    {sanitizedStacktrace}")
                .AppendLine($"{indent}TargetSite : '{exception.TargetSite}");

            if (exception.InnerException != null)
                logs.AppendLine($"{indent}InnerEx    : ");

            FormatException(exception.InnerException, logs, tabs + 1);
        }

        private static void FormatData(IDictionary dict, StringBuilder logs, int tabs)
        {
            if (dict == null)
                return;

            var indent = new string(' ', (tabs + 1) * TabSize);

            foreach (var val in dict.Keys)
                logs.Append($"{Environment.NewLine}{indent}'{val}' : '{dict[val]}'");
        }
    }
}
