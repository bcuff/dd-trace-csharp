using NUnit.Framework;
using System;
using FluentAssertions;

namespace DataDog.Tracing.Tests
{
    [TestFixture]
    public class SpanTests
    {
        [Test]
        public void Span_CreateChild_should_populate_expected_fields()
        {
            var trace = new RootSpan();
            var child = (Span)trace.Begin("foobar", "some-service", "some-resource", "some-type");
            child.Name.Should().Be("foobar");
            child.Service.Should().Be("some-service");
            child.Resource.Should().Be("some-resource");
            child.Type.Should().Be("some-type");
            child.ParentId.Should().Be(trace.SpanId);
            child.TraceId.Should().Be(trace.TraceId);

            var grandchild = (Span)child.Begin("grandchild", "some-service2", "some-resource2", "some-type2");
            grandchild.Name.Should().Be("grandchild");
            grandchild.Service.Should().Be("some-service2");
            grandchild.Resource.Should().Be("some-resource2");
            grandchild.Type.Should().Be("some-type2");
            grandchild.ParentId.Should().Be(child.SpanId);
            grandchild.TraceId.Should().Be(trace.TraceId);
        }

        [Test]
        [TestCase("test-key", "test-value")]
        [TestCase("123", "345")]
        public void Span_SetMeta_should_populate_expected_meta(string key, string value)
        {
            var trace = new RootSpan();
            trace.SetMeta(key, value);
            trace.Meta.Should().NotBeNull();
            trace.Meta.ContainsKey(key).Should().BeTrue();
            trace.Meta[key].Should().Be(value);
        }

        [Test]
        public void Span_end_should_seal_span()
        {
            void AssertSeals(ISpan span)
            {
                span.Invoking(t => t.SetMeta("test", "test")).ShouldNotThrow();
                span.Invoking(t => t.SetError(new Exception())).ShouldNotThrow();
                span.Invoking(t => t.Begin("foo", "bar", "foo", "bar")).ShouldNotThrow();

                span.Dispose();

                span.Invoking(t => t.SetMeta("test", "test")).ShouldThrow<InvalidOperationException>();
                span.Invoking(t => t.SetError(new Exception())).ShouldThrow<InvalidOperationException>();
                span.Invoking(t => t.Begin("foo", "bar", "foo", "bar")).ShouldThrow<InvalidOperationException>();
                span.Invoking(t => t.Dispose()).ShouldThrow<InvalidOperationException>();
            }

            var trace = new RootSpan();
            AssertSeals(trace);

            trace = new RootSpan();
            var child = trace.Begin("a", "a", "a", "a");
            AssertSeals(child);
        }

        [Test]
        public void Span_SetError_should_set_expected_meta()
        {
            var trace = new RootSpan();
            Exception x;
            try
            {
                throw new Exception("bang!");
            }
            catch (Exception ex)
            {
                x = ex;
            }
            trace.SetError(x);
            trace.Meta.Should().NotBeNull();
            trace.Meta["error.msg"].Should().Be("bang!");
            trace.Meta["error.type"].Should().Be(nameof(Exception));
            trace.Meta["error.stack"].Should().Be(x.StackTrace);

            trace.SetError(new InvalidOperationException("2"));
            trace.Meta["error.msg"].Should().Be("2");
            trace.Meta["error.type"].Should().Be(nameof(InvalidOperationException));
            trace.Meta.ContainsKey("error.stack").Should().BeFalse();
        }
    }
}
