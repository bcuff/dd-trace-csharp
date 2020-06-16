using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    sealed class RootSpan : Span
    {
        readonly int? _maxSpans;
        readonly IObserver<Trace> _observer;

        public RootSpan()
        {
            Spans.Add(this);
        }

        public RootSpan(IObserver<Trace> observer, int? maxSpans)
            : this()
        {
            _maxSpans = maxSpans;
            _observer = observer;
        }

        protected override void OnBeginChild(Span child)
        {
            lock (this)
            {
                EnsureNotSealed();
                if (_maxSpans == null || Spans.Count < _maxSpans)
                {
                    Spans.Add(child);
                }
            }
            base.OnBeginChild(child);
            child.BeginChild += OnBeginChild;
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            _observer?.OnNext(new Trace(this));
        }
 
        [JsonIgnore]
        public List<Span> Spans { get; } = new List<Span>();
   }
}
