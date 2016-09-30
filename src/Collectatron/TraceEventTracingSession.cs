#if NET451
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace Collectatron
{
    public class TraceEventTracingSession : TracingSession
    {
        private readonly List<string> _providers = new List<string>();
        private TraceEventSession _session;

        public override void EnableProvider(string provider)
        {
            _providers.Add(provider);
        }

        public override bool Start()
        {
            var etlFile = Path.Combine(OutputDirectory, "events.etl");

            _session = new TraceEventSession("Collectatron", etlFile);
            _session.BufferSizeMB = 512;

            // For now, we don't allow configuring kernel providers
            _session.EnableKernelProvider(
                KernelTraceEventParser.Keywords.ImageLoad |
                KernelTraceEventParser.Keywords.Process |
                KernelTraceEventParser.Keywords.Thread);

            foreach(var provider in _providers)
            {
                _session.EnableProvider(provider);
            }

            return true;
        }

        public override bool Stop()
        {
            var eventsLost = _session.EventsLost;

            _session.Stop();

            Console.WriteLine($"Tracing session stopped. Events Lost: {eventsLost}");

            return true;
        }

        public override void Dispose()
        {
            _session?.Dispose();
            _session = null;
        }
    }
}
#endif
