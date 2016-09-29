using System.Diagnostics.Tracing;

namespace Collectatron
{
    [EventSource]
    public class CollectionControlEventSource: EventSource
    {
        public static readonly CollectionControlEventSource Log = new CollectionControlEventSource();
        private CollectionControlEventSource() : base("Microsoft-AspNetCore-Benchmarking-CollectionControl") { }

        [Event(1)]
        public void StartCollection() { WriteEvent(1); }

        [Event(2)]
        public void StopCollection() { WriteEvent(2); }
    }
}
