using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    sealed class Span : ISpan
    {
        readonly TraceService _service;
        bool _sealed;

        public Span(TraceService service)
        {
            _service = service;
        }

        [JsonProperty("trace_id")]
        public long TraceId { get; set; }
        [JsonProperty("span_id")]
        public long SpanId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("resource")]
        public string Resource { get; set; }
        [JsonProperty("service")]
        public string Service { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("start")]
        public long Start { get; set; }
        [JsonProperty("duration")]
        public long Duration { get; set; }
        [JsonProperty("parent_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long? ParentId { get; set; }
        [JsonProperty("error", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Error { get; set; }
        [JsonProperty("meta", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> Meta { get; set; }

        public void Dispose()
        {
            lock (this)
            {
                EnsureNotSealed();
                Duration = Util.GetTimestamp() - Start;
                _sealed = true;
            }
            _service.Post(this);
        }

        private void EnsureNotSealed()
        {
            if (_sealed)
            {
                throw new InvalidOperationException("This span has already ended.");
            }
        }

        public ISpan Begin(string name, string resource, string type) => new Span(_service)
        {
            TraceId = TraceId,
            SpanId = Util.NewSpanId(),
            Name = name,
            Resource = resource,
            ParentId = SpanId,
            Type = type,
            Service = Service,
            Start = Util.GetTimestamp(),
        };

        public void SetMeta(string name, string value)
        {
            lock (this)
            {
                EnsureNotSealed();
                (Meta ?? (Meta = new Dictionary<string, string>()))[name] = value;
            }
        }

        public void SetError(Exception ex)
        {
            lock (this)
            {
                EnsureNotSealed();
                var meta = Meta ?? (Meta = new Dictionary<string, string>());
                Error = 1;
                meta["error.msg"] = ex.Message;
                meta["error.type"] = ex.GetType().Name;
                meta["error.stack"] = ex.StackTrace;
            }
        }
    }
}
