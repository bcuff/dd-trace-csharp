using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    /// <summary>
    /// Sets up the TraceContext.Current with the specified trace.
    /// </summary>
    public class TraceContextScope : IDisposable
    {
        readonly Span _span;

        /// <summary>
        /// Sets up the current span on <c>TraceContext.Current</c> within an async local context.
        /// </summary>
        /// <param name="span">The span.</param>
        public TraceContextScope(ISpan span)
        {
            if (span == null) throw new ArgumentNullException(nameof(span));
            var t = span as RootSpan;
            _span = t ?? throw new ArgumentException($"{nameof(span)} must originate from a {nameof(TraceSource)} instance.");
            SetupEvents(t);
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

        public void Dispose() => _span.Dispose();
    }
}
