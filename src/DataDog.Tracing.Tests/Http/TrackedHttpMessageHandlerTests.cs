using System;
using System.Net.Http;
using System.Threading.Tasks;
using DataDog.Tracing.Http;
using FluentAssertions;
using NUnit.Framework;

namespace DataDog.Tracing.Tests.Http
{
    [TestFixture]
    public class TrackedHttpMessageHandlerTests
    {
        Trace _lastTrace;
        HttpClient _client;
        TraceSource _traceSource;
        HttpMessageHandler _innerHandler;
        TraceHttpMessageHandler _traceHandler;

        [SetUp]
        public void SetUp()
        {
            _traceSource = new TraceSource();
            _traceSource.Subscribe(t => _lastTrace = t);
            _innerHandler = new HttpClientHandler();
            _traceHandler = new TraceHttpMessageHandler(_innerHandler, _traceSource);
            _client = new HttpClient(_traceHandler);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _traceHandler.Dispose();
            _innerHandler.Dispose();
        }

        [Test]
        [TestCase("https://google.com/", 200)]
        [TestCase("https://google.com/404", 404)]
        public async Task TraceHandler_should_record_expected_status_code(string url, int statusCode)
        {
            var response = await _client.GetAsync(url);
            ((int)response.StatusCode).Should().Be(statusCode);
            _lastTrace.Should().NotBeNull();
            _lastTrace.Root.Spans[0].Name.Should().Be($"HTTP GET");
            _lastTrace.Root.Spans[0].Service.Should().Be("http");
            _lastTrace.Root.Spans[0].Type.Should().Be("http");
            _lastTrace.Root.Spans[0].Resource.Should().Be(new Uri(url).Host);
            _lastTrace.Root.Spans[0].Meta["http.url"].Should().Be(url);
            _lastTrace.Root.Spans[0].Meta["http.status_code"].Should().Be(statusCode.ToString());
        }
    }
}
