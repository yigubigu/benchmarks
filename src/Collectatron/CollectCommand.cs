using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public class CollectCommand
    {
        private CommandArgument _pidArgument;
        private CommandOption _outputOption;
        private CommandOption _providerOption;
        private CommandOption _forceOption;
        private CommandOption _clrOption;
        private TracingSession _session;

        public void Register(CommandLineApplication app)
        {
            _session = TracingSession.Create();

            var clrTracingValues = string.Join(",", Enum.GetValues(typeof(ClrTracingMode)));

            _pidArgument = app.Argument("<PROCESS_ID>", "The PID of the process to collect for");
            _outputOption = app.Option("-o|--output <OUTPUTNAME>", "Specifies the name of the output reports directory to use", CommandOptionType.SingleValue);
            _forceOption = app.Option("-f|--force", "Indicates that existing results in <OUTPUTNAME> should be deleted", CommandOptionType.NoValue);
            _providerOption = app.Option("--provider <PROVIDER>", "Specifies an event Provider to enable.", CommandOptionType.MultipleValue);
            _clrOption = app.Option("-c|--clr-events <CLR_EVENTS>", $"Specifies the CLR event tracing mode to use. Default: '{nameof(ClrTracingMode.Default)}', Possible Values: " + clrTracingValues, CommandOptionType.SingleValue);

            _session.AttachArguments(app);

            app.OnExecute(() => Execute());
        }

        public int Execute()
        {
            if (string.IsNullOrEmpty(_pidArgument.Value))
            {
                Console.Error.WriteLine("Missing required argument: <PROCESS_ID>");
                return 1;
            }

            var output = Path.GetFullPath(_outputOption.HasValue() ? _outputOption.Value() : Path.Combine(Directory.GetCurrentDirectory(), "collection"));

            if (Directory.Exists(output))
            {
                if (_forceOption.HasValue())
                {
                    Directory.Delete(output, recursive: true);
                }
                else
                {
                    Console.Error.WriteLine($"Results already exist in '{output}'. Use '-f' to force overwriting them");
                    return 1;
                }
            }
            Directory.CreateDirectory(output);

            var target = Process.GetProcessById(int.Parse(_pidArgument.Value));

            _session.Initialize(output, target);

            var clrTracingMode = _clrOption.HasValue() ?
                (ClrTracingMode)Enum.Parse(typeof(ClrTracingMode), _clrOption.Value(), ignoreCase: true) :
                ClrTracingMode.Default;

            _session.EnableClrTracing(clrTracingMode);

            foreach (var provider in _providerOption.Values)
            {
                _session.EnableProvider(provider);
            }

            // Start the session
            if (!_session.Start())
            {
                return 1;
            }

            // Wait for the user to terminate
            Console.WriteLine("Collection started, press 'S' to stop");
            while (Console.ReadKey(intercept: true).Key != ConsoleKey.S) { }

            // Shut down the session
            if (!_session.Stop())
            {
                return 1;
            }
            return 0;
        }
    }
}