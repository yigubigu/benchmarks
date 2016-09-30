using System;
using System.Diagnostics;
using Microsoft.Extensions.CommandLineUtils;

namespace Collectatron
{
    public abstract class TracingSession : IDisposable
    {
        public string OutputDirectory { get; private set; }

        public static TracingSession Create()
        {
#if NET451
            return new TraceEventTracingSession();
#else
            return new LTTngTracingSession();
#endif
        }

        public virtual void AttachArguments(CommandLineApplication app) { }

        public virtual void Initialize(string outputDirectory)
        {
            OutputDirectory = outputDirectory;
        }

        public abstract void EnableProvider(string provider);
        public abstract bool Start();
        public abstract bool Stop();
        public abstract void Dispose();
    }
}