using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    internal class TraceContextScope : IDisposable
    {
        readonly Trace _trace;

        public TraceContextScope(Trace trace)
        {
            _trace = trace;
            SetupEvents(trace);
        }

        private static void SetupEvents(Span span)
        {
            var before = TraceContext.Current;
            TraceContext.Current = span;
            span.BeginChild += SetupEvents;
            span.End += () =>
            {
                TraceContext.Current = before;
            };
        }

        public void Dispose() => _trace.Dispose();
    }
}
