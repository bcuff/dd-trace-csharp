using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;

namespace DataDog.Tracing
{
    public static class TraceContext
    {
        static readonly AsyncLocal<ISpan> _current = new AsyncLocal<ISpan>();

        /// <summary>
        /// Gets the current span or <c>null</c> if no span was set up.
        /// </summary>
        public static ISpan Current
        {
            get => _current.Value;
            internal set => _current.Value = value;
        }

        /// <summary>
        /// Clears the TraceContext from the current scope.
        /// This is useful for protecting things that shouldn't have access to it. e.g. background work.
        /// </summary>
        public static void Reset() => _current.Value = null;
    }
}
