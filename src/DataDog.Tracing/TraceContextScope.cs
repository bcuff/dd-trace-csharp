﻿using System;

namespace DataDog.Tracing
{
    /// <summary>
    /// Sets up the TraceContext.Current with the specified trace.
    /// </summary>
    public class TraceContextScope : IDisposable
    {
        private readonly ISpan _old;
        private ISpan _span;

        public TraceContextScope(ISpan span)
        {
            _span = span;
            _old = TraceContext.Current;
            TraceContext.Current = span;
        }

        public void Dispose()
        {
            var span = _span;
            _span = null;
            var c = TraceContext.Current;
            if (c != span) throw new InvalidOperationException("Overlapped scopes are not allowed.");
            TraceContext.Current = _old;
        }
    }
}
