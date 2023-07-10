using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Sinks.Http.BatchFormatters;

namespace pdf_page_splitter.Logging
{
    internal class LogDnaBatchFormatter : BatchFormatter
    {
        public LogDnaBatchFormatter(long? eventBodyLimitBytes = 256 * 1024) : base(eventBodyLimitBytes)
        {
        }

        public override void Format(IEnumerable<string> logEvents, TextWriter output)
        {
            _ = logEvents ?? throw new ArgumentNullException(nameof(logEvents));
            _ = output ?? throw new ArgumentNullException(nameof(output));

            if (!logEvents.Any())
                return; // abort

            output.Write("{\"lines\":[");

            var delimStart = string.Empty;

            foreach (var logEvent in logEvents)
            {
                if (string.IsNullOrWhiteSpace(logEvent))
                    continue;

                if (this.CheckEventBodySize(logEvent))
                {
                    output.Write(delimStart);
                    output.Write(logEvent);
                    delimStart = ",";
                }
            }

            output.Write("]}");
        }
    }
}
