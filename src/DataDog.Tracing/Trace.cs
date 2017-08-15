using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    sealed class Trace : BaseSpan
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

        protected override BaseSpan CreateChild()
        {
            var result = new Span(this);
            lock (this)
            {
                EnsureNotSealed();
                Spans.Add(result);
            }
            return result;
        }

        [JsonIgnore]
        public List<BaseSpan> Spans { get; } = new List<BaseSpan>();

        protected override void OnEnd()
        {
            _service?.Post(this);
        }
    }
}
