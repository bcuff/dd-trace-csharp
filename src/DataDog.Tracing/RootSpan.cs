using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    sealed class RootSpan : Span
    {
        readonly IObserver<Trace> _observer;

        public RootSpan()
        {
            Spans.Add(this);
        }

        public RootSpan(IObserver<Trace> observer)
            : this()
        {
            _observer = observer;
        }

        protected override void OnBeginChild(Span child)
        {
            lock (this)
            {
                EnsureNotSealed();
                Spans.Add(child);
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
