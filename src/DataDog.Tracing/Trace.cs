using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    sealed class Trace : Span
    {
        readonly TraceService _service;

        public Trace()
        {
            Spans.Add(this);
        }

        public Trace(TraceService service)
            : this()
        {
            _service = service;
        }

        protected override void OnBeginChild(Span child)
        {
            lock (this)
            {
                EnsureNotSealed();
                Spans.Add(child);
            }
            child.BeginChild += OnBeginChild;
            base.OnBeginChild(child);
        }

        protected override void OnEnd()
        {
            base.OnEnd();
            _service?.Post(this);
        }
 
        [JsonIgnore]
        public List<Span> Spans { get; } = new List<Span>();
   }
}
