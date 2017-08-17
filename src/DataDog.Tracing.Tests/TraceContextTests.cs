using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace DataDog.Tracing.Tests
{
    [TestFixture]
    public class TraceContextTests
    {
        [Test]
        public async Task TraceContextScope_should_install_current_and_work_recursively()
        {
            var trace = new RootSpan();
            TraceContext.Current.Should().BeNull();
            using (new TraceContextScope(trace))
            {
                TraceContext.Current.Should().NotBeNull();
                TraceContext.Current.Should().Be(trace);

                Span s1 = null, s2 = null, s3 = null;
                var t1 = SomeAsyncTask(async () => s1 = (Span)TraceContext.Current);
                TraceContext.Current.Should().Be(trace);
                var t2 = SomeAsyncTask(async () =>
                {
                    s2 = (Span)TraceContext.Current;
                    await SomeAsyncTask(async () => s3 = (Span)TraceContext.Current);
                });
                TraceContext.Current.Should().Be(trace);
                await Task.WhenAll(t1, t2);
                TraceContext.Current.Should().Be(trace);

                s1.Should().NotBeNull();
                s1.TraceId.Should().Be(trace.TraceId);
                s1.ParentId.Should().Be(trace.SpanId);

                s2.Should().NotBeNull();
                s2.TraceId.Should().Be(trace.TraceId);
                s2.ParentId.Should().Be(trace.SpanId);

                s3.Should().NotBeNull();
                s3.TraceId.Should().Be(trace.TraceId);
                s3.ParentId.Should().Be(s2.SpanId);

                new [] { s1.SpanId, s2.SpanId, s3.SpanId }.Distinct().Count().Should().Be(3);
                trace.Spans.Count.Should().Be(4);
                trace.Spans.Select(s => s.SpanId).Distinct().Count().Should().Be(4);
                trace.Spans.Should().Contain(new[] { trace, s1, s2, s3 });
            }
            TraceContext.Current.Should().BeNull();
        }

        private async Task SomeAsyncTask(Func<Task> inner)
        {
            using (TraceContext.Current.Begin("some-async-task", "something", "resource", "test"))
            {
                await Task.CompletedTask;
                await inner();
            }
        }
    }
}
