using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Collectatron
{
    public class PerfView
    {
        public static readonly PerfView Default = new PerfView(FindPerfView());

        public string ExePath { get; }

        public PerfView(string exePath)
        {
            ExePath = exePath;
        }

        public Session Collect(List<string> arguments)
        {
            var session = Session.Create(ExePath, arguments);
            session.Start();
            return session;
        }

        private static string FindPerfView()
        {
            var candidate = Environment.GetEnvironmentVariable("PERFVIEW_PATH");
            if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(AppContext.BaseDirectory, "PerfView.exe");
            if(File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(Directory.GetCurrentDirectory(), "PerfView.exe");
            if(File.Exists(candidate))
            {
                return candidate;
            }

            throw new FileNotFoundException("Unable to locate PerfView.exe");
        }

        public class Session : IDisposable
        {
            private Process _process;

            public Session(Process process)
            {
                _process = process;
            }

            internal static Session Create(string exePath, List<string> arguments)
            {
                var process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.Arguments = "collect " + string.Join(" ", arguments);
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                return new Session(process);
            }

            public void Start()
            {
                _process.OutputDataReceived += OnOutputDataReceived;
                _process.ErrorDataReceived += OnErrorDataReceived;

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            public bool Stop()
            {
                Console.WriteLine("Stopping PerfView...");
                CollectionControlEventSource.Log.StopCollection();
                _process.WaitForExit();
                return true;
            }

            private static void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                Console.WriteLine(e.Data);
            }

            private static void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                Console.Error.WriteLine(e.Data);
            }

            public void Dispose()
            {
                _process.Kill();
                _process.Dispose();
            }
        }
    }
}