using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public abstract class TracingSession : IDisposable
    {
        public Process TargetProcess { get; private set; }
        public string OutputDirectory { get; private set; }

        public static TracingSession Create()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new PerfViewTracingSession();
            }
            else
            {
                return new LTTngTracingSession();
            }
        }

        public virtual void AttachArguments(CommandLineApplication app) { }

        public virtual void Initialize(string outputDirectory, Process process)
        {
            OutputDirectory = outputDirectory;
            TargetProcess = process;
        }

        public abstract void EnableProvider(string provider);
        public abstract void EnableClrTracing(ClrTracingMode mode);
        public abstract bool Start();
        public abstract bool Stop();
        public abstract void Dispose();
    }
}