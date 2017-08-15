using System;
using System.Collections.Generic;
using System.Linq;
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

        static byte[] SerializeTraces(Trace trace)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, _encoding))
            {
                _serializer.Serialize(writer, new [] { trace.Spans });
                writer.Flush();
                return ms.ToArray();
            }
        }

        readonly string _serviceName;
        readonly ILogger _logger;
        readonly ITargetBlock<Trace> _block;
        readonly Task _shutdownTask;
        readonly HttpClient _client = new HttpClient();

        public TraceService(string serviceName)
            : this(serviceName, null, null)
        {
        }

        public TraceService(string serviceName, Uri baseUrl)
            : this(serviceName, baseUrl, null)
        {
        }

        public TraceService(string serviceName, Uri baseUrl, ILogger logger)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _client.BaseAddress = baseUrl ?? new Uri("http://localhost:8126");
            _logger = logger;
            var transform = new TransformBlock<Trace, byte[]>((Func<Trace, byte[]>)SerializeTraces);
            var send = new ActionBlock<byte[]>(PutTraces);
            transform.LinkTo(send, new DataflowLinkOptions { PropagateCompletion = true });
            _shutdownTask = send.Completion;
            _block = transform;
        }

        public ISpan BeginTrace(string name, string resource, string type) => new Trace(this)
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
                    Console.WriteLine("PUT responsed with " + response.StatusCode);
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
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

        internal void Post(Trace trace) => _block.Post(trace);

        public Task ShutdownAsync()
        {
            _block.Complete();
            return _shutdownTask;
        }
    }
}
