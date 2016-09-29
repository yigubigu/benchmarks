using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public class PerfViewTracingSession : TracingSession
    {
        private List<string> _providers = new List<string>();
        private string _clrTracingMode = "Default";
        private CommandOption _perfViewPathOption;
        private PerfView.Session _session;

        public override void AttachArguments(CommandLineApplication app)
        {
            _perfViewPathOption = app.Option("--perfview <PATH_TO_PERFVIEW>", "Specifies the path to the PerfView executable", CommandOptionType.SingleValue);
        }

        public override void EnableProvider(string provider)
        {
            _providers.Add(provider);
        }

        public override void EnableClrTracing(ClrTracingMode mode)
        {
            // Map the input value to the PerfView string.
            switch (mode)
            {
                case ClrTracingMode.None:
                    _clrTracingMode = "None";
                    break;
                case ClrTracingMode.Default:
                    _clrTracingMode = "Default";
                    break;
                case ClrTracingMode.All:
                    _clrTracingMode = "All";
                    break;
                default:
                    throw new InvalidOperationException("Unknown Clr Tracing mode: " + mode.ToString());
            }
        }

        public override bool Start()
        {
            // Create PerfView helper
            var perfView = _perfViewPathOption.HasValue() ? new PerfView(_perfViewPathOption.Value()) : PerfView.Default;
            Console.WriteLine("Using PerfView.exe from: " + perfView.ExePath);

            // Build arguments to PerfView
            var arguments = new List<string>();
            arguments.Add(Path.Combine(OutputDirectory, "Collection.etl"));
            arguments.Add("-Merge");
            arguments.Add("-NoGui");
            arguments.Add("-NoNGenRundown"); // <-- NGen rundown only needed pre-.NET 4.5

            // Add providers
            var collectionControlProviderGuid = EventSource.GetGuid(typeof(CollectionControlEventSource)).ToString();
            _providers.Add(collectionControlProviderGuid);
            if (_providers.Any())
            {
                var providerString = string.Join(",", _providers);
                arguments.Add($"-Providers:\"{providerString}\"");
            }

            // Set CLR tracing mode
            arguments.Add($"-ClrEvents:{_clrTracingMode}");

            // Configure stopping behavior
            arguments.Add($"-StopOnEtwEvent:{collectionControlProviderGuid}/EventId(2);Process={Process.GetCurrentProcess().Id}");

            // Start collection
            _session = perfView.Collect(arguments);

            return true;
        }

        public override bool Stop() => _session.Stop();

        public override void Dispose()
        {
            _session?.Dispose();
        }
    }
}
