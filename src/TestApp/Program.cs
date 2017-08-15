using System;
using System.Threading;
using DataDog.Tracing;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new TraceService("test-app");
            using (var span = service.BeginTrace("sample-trace", "Main", "console"))
            {
                TestSpan(span);
                TestSpan(span);
                TestSpan(span);
            }
            service.ShutdownAsync().Wait();
        }

        private static void TestSpan(ISpan span)
        {
            using (var child = span.Begin("GET something", "memcached", "GET", "cache"))
            {
                child.SetMeta("memcached_key", "test123");
                Thread.Sleep(100);
            }
        }
    }
}