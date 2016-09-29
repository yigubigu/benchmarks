using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public class Program
    {
        private static readonly Assembly Asm = typeof(Program).GetTypeInfo().Assembly;
        private static readonly string Version = Asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        public static int Main(string[] args)
        {
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
