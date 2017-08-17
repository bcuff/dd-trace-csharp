using System;
using System.Reactive.Linq;
using System.Threading;
using DataDog.Tracing;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = new TraceSource();
            var agent = new TraceAgent();
            using (source.Subscribe(agent))
            using (var span = source.BeginTrace("sample-trace", "test-app", "Main", "console"))
            {
                TestSpan(span);
                TestSpan(span);
                TestSpan(span);
            }
            agent.OnCompleted();
            agent.Completion.Wait();
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