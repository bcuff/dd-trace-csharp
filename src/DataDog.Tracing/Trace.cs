using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    sealed class Trace : BaseSpan
    {
        readonly TraceService _service;

        public Trace(TraceService service)
        {
            _service = service;
            Spans.Add(this);
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

        public List<BaseSpan> Spans { get; } = new List<BaseSpan>();

        protected override void OnEnd()
        {
            _service.Post(this);
        }
    }
}
