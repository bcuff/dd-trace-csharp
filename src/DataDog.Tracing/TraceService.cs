using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataDog.Tracing
{
    public class TraceService
    {
        static readonly Encoding _encoding = new UTF8Encoding(false);
        static readonly JsonSerializer _serializer = new JsonSerializer();
        static readonly MediaTypeHeaderValue _contentHeader = new MediaTypeHeaderValue("application/json");

        static byte[] SerializeSpans(Span[] spans)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, _encoding))
            {
                _serializer.Serialize(writer, new { spans });
                writer.Flush();
                return ms.ToArray();
            }
        }

        readonly string _serviceName;
        readonly ILogger _logger;
        readonly BatchBlock<Span> _block = new BatchBlock<Span>(20);
        readonly HttpClient _client = new HttpClient();

        public TraceService(string serviceName, Uri baseUrl = null, ILogger logger = null)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _client.BaseAddress = baseUrl ?? new Uri("http://localhost:8126");
            _logger = logger;
            var transform = new TransformBlock<Span[], byte[]>((Func<Span[], byte[]>)SerializeSpans);
            var send = new ActionBlock<byte[]>(PutTraces);
            _block.LinkTo(transform, new DataflowLinkOptions { PropagateCompletion = true });
            transform.LinkTo(send, new DataflowLinkOptions { PropagateCompletion = true });
        }

        public ISpan BeginTrace(string name, string resource, string type) => new Span(this)
        {
            TraceId = Util.NewTraceId(),
            SpanId = Util.NewSpanId(),
            Name = name,
            Resource = resource,
            Type = type,
            Service = _serviceName,
            Start = Util.GetTimestamp(),
        };

        private async Task PutTraces(byte[] tracesBody)
        {
            try
            {
                var content = new ByteArrayContent(tracesBody);
                content.Headers.ContentType = _contentHeader;
                using (var response = await _client.PutAsync("/v0.3/traces", content))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger?.LogError($"HTTP {response.StatusCode} from PUT /v0.3/traces");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "PUT /v0.3/traces failed");
            }
        }

        internal void Post(Span span) => _block.Post(span);

        public Task ShutdownAsync()
        {
            _block.Complete();
            return _block.Completion;
        }
    }
}
