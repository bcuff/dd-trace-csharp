using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace DataDog.Tracing.Tests
{
    [TestFixture]
    public class TraceContextTests
    {
        [Test]
        public async Task TraceContextScope_should_install_current()
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

                s1.Should().Be(trace);
                s2.Should().Be(trace);
                s3.Should().Be(trace);
            }
            TraceContext.Current.Should().BeNull();
        }

        [Test]
        public async Task TraceContextScope_should_install_current_and_work_recursively()
        {
            RootSpan trace;
            using (trace = new RootSpan())
            {
                TraceContext.Current.Should().BeNull();
                await SomeAsyncTask(async () => { TraceContext.Current.Should().BeNull(); });
                using (new TraceContextScope(trace))
                {
                    TraceContext.Current.Should().Be(trace);
                    await SomeAsyncTask(async () => { TraceContext.Current.Should().Be(trace); });
                    using (var span = trace.Begin("child", "child", "child", "child"))
                    {
                        TraceContext.Current.Should().Be(trace);
                        await SomeAsyncTask(async () => { TraceContext.Current.Should().Be(trace); });
                        using (new TraceContextScope(span))
                        {
                            TraceContext.Current.Should().Be(span);
                            await SomeAsyncTask(async () => { TraceContext.Current.Should().Be(span); });
                        }
                        TraceContext.Current.Should().Be(trace);
                        await SomeAsyncTask(async () => { TraceContext.Current.Should().Be(trace); });
                    }
                    TraceContext.Current.Should().Be(trace);
                    await SomeAsyncTask(async () => { TraceContext.Current.Should().Be(trace); });
                }
                trace.Sealed.Should().BeFalse(); // scope shouldn't dispose the trace
                TraceContext.Current.Should().BeNull();
                await SomeAsyncTask(async () => { TraceContext.Current.Should().BeNull(); });
            }
            trace.Sealed.Should().BeTrue();
            TraceContext.Current.Should().BeNull();
            await SomeAsyncTask(async () => { TraceContext.Current.Should().BeNull(); });
        }

        private async Task SomeAsyncTask(Func<Task> inner)
        {
            using (TraceContext.Current?.Begin("some-async-task", "something", "resource", "test"))
            {
                await Task.CompletedTask;
                await inner();
            }
        }
    }
}
