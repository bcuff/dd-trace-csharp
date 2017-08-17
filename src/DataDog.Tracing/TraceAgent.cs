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
    public class TraceAgent : IObserver<Trace>
    {
        static readonly Encoding _encoding = new UTF8Encoding(false);
        static readonly JsonSerializer _serializer = new JsonSerializer();
        static readonly MediaTypeHeaderValue _contentHeader = new MediaTypeHeaderValue("application/json");

        static byte[] SerializeTraces(RootSpan trace)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, _encoding))
            {
                _serializer.Serialize(writer, new [] { trace.Spans });
                writer.Flush();
                return ms.ToArray();
            }
        }

        readonly ILogger _logger;
        readonly ITargetBlock<RootSpan> _block;
        readonly HttpClient _client = new HttpClient();

        public TraceAgent()
            : this(null, null)
        {
        }

        public TraceAgent(Uri baseUrl)
            : this(baseUrl, null)
        {
        }

        public TraceAgent(Uri baseUrl, ILogger logger)
        {
            _client.BaseAddress = baseUrl ?? new Uri("http://localhost:8126");
            _logger = logger;
            var transform = new TransformBlock<RootSpan, byte[]>((Func<RootSpan, byte[]>)SerializeTraces);
            var send = new ActionBlock<byte[]>(PutTraces);
            transform.LinkTo(send, new DataflowLinkOptions { PropagateCompletion = true });
            _block = transform;
            Completion = send.Completion;
        }

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

        public Task Completion { get; }

        public void OnCompleted()
        {
            _block.Complete();
        }

        public void OnError(Exception error)
        {
            _block.Fault(error);
        }

        public void OnNext(Trace value)
        {
            _block.Post(value.Root);
        }
    }
}
