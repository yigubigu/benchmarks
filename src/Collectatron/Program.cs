using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public class Program
    {
        private static readonly Assembly Asm = typeof(Program).GetTypeInfo().Assembly;
        private static readonly string Version = Asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        public static int Main(string[] args)
        {
#if NETCOREAPP1_0
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Error.WriteLine("Due to the TraceEvent library only supporting Windows via the Desktop CLR");
                Console.Error.WriteLine("The Desktop CLR version of this tool must be used on Windows.");
                Console.Error.WriteLine("See https://github.com/Microsoft/perfview/issues/121 for updates");
                return 1;
            }
#else
            if(Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Console.Error.WriteLine("The Desktop CLR build of this tool is not designed to run in Mono");
                return 1;
            }
#endif

#if DEBUG
            if(args.Any(s => s.Equals("--debug")))
            {
                Console.WriteLine($"Waiting for debugger. Process ID: {Process.GetCurrentProcess().Id}");
                Console.WriteLine("Press ENTER to continue");
                Console.ReadLine();
                args = args.Where(s => !s.Equals("--debug")).ToArray();
            }
#endif

            var app = new CommandLineApplication();
            app.Name = Asm.GetName().Name;
            app.FullName = "The Collectatron 3000";
            app.Description = "CLR Event Collection Tool";
            app.VersionOption("-v|--version", Version);
            app.HelpOption("-h|-?|--help");

            app.Command("collect", new CollectCommand().Register);

            app.Command("help", help =>
            {
                var command = help.Argument("<COMMAND>", "The command to get help for");

                help.OnExecute(() =>
                {
                    app.ShowHelp(command.Value);
                    return 0;
                });
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            return app.Execute(args);
        }
    }
}
