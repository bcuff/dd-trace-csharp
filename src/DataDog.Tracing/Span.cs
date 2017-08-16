using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    class Span : ISpan
    {
        public event Action<Span> BeginChild;
        public event Action End;

        protected bool Sealed;

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

        protected virtual void OnBeginChild(Span child)
        {
            BeginChild?.Invoke(child);
        }

        protected virtual void OnEnd()
        {
            End?.Invoke();
        }

        public void Dispose()
        {
            lock (this)
            {
                EnsureNotSealed();
                Duration = Util.GetTimestamp() - Start;
                Sealed = true;
            }
            OnEnd();
        }

        protected void EnsureNotSealed()
        {
            if (Sealed)
            {
                throw new InvalidOperationException("This span has already ended.");
            }
        }

        public ISpan Begin(string name, string serviceName, string resource, string type)
        {
            EnsureNotSealed();
            var child = new Span();
            child.TraceId = TraceId;
            child.SpanId = Util.NewSpanId();
            child.Name = name;
            child.Resource = resource;
            child.ParentId = SpanId;
            child.Type = type;
            child.Service = serviceName;
            child.Start = Util.GetTimestamp();
            OnBeginChild(child);
            return child;
        }

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
                var stack = ex.StackTrace;
                if (stack == null)
                {
                    meta.Remove("error.stack");
                }
                else
                {
                    meta["error.stack"] = stack;
                }
            }
        }
    }
}
