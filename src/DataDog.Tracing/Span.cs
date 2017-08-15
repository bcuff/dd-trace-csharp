using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    class Span : BaseSpan
    {
        readonly Trace _trace;

        public Span(Trace trace)
        {
            _trace = trace;
        }

        public void ForceEnd()
        {
            if (Sealed) return;
            lock (this)
            {
                if (Sealed) return;
                var spanEnd = _trace.Start + _trace.Duration;
                Duration = spanEnd - Start;
                (Meta ?? (Meta = new Dictionary<string, string>()))["incomplete"] = "true";
            }
        }

        protected override BaseSpan CreateChild() => new Span(_trace);

        protected override void OnEnd()
        {
        }
    }
}
