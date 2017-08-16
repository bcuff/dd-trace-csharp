using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DataDog.Tracing
{
    public static class TraceContext
    {
        static readonly AsyncLocal<ISpan> _current = new AsyncLocal<ISpan>();

        public static ISpan Current
        {
            get => _current.Value;
            internal set => _current.Value = value;
        }
    }
}
