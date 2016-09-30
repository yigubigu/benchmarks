using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public class CollectCommand
    {
        private CommandOption _outputOption;
        private CommandOption _providerOption;
        private CommandOption _forceOption;
        private CommandOption _clrOption;
        private TracingSession _session;

        public void Register(CommandLineApplication app)
        {
            _session = TracingSession.Create();

            _outputOption = app.Option("-o|--output <OUTPUTNAME>", "Specifies the name of the output reports directory to use", CommandOptionType.SingleValue);
            _forceOption = app.Option("-f|--force", "Indicates that existing results in <OUTPUTNAME> should be deleted", CommandOptionType.NoValue);
            _providerOption = app.Option("--provider <PROVIDER>", "Specifies an event Provider to enable.", CommandOptionType.MultipleValue);

            _session.AttachArguments(app);

            app.OnExecute(() => Execute());
        }

        public int Execute()
        {
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

            _session.Initialize(output);

            foreach (var provider in _providerOption.Values)
            {
                _session.EnableProvider(provider);
            }

            // Make sure we dispose of the session even if we're terminated by Ctrl-C.
            // If we're terminated outside of Ctrl-C, well we're screwed there :).
            Console.CancelKeyPress += (sender, e) => _session.Dispose();

            // Start the session
            if (!_session.Start())
            {
                return 1;
            }

            // Wait for the user to terminate
            Console.WriteLine("Collection started, press 'S' to stop");
            while (Console.ReadKey(intercept: true).Key != ConsoleKey.S) { }
            Console.WriteLine("Shutting down collection...");

            // Shut down the session
            if (!_session.Stop())
            {
                return 1;
            }
            return 0;
        }
    }
}