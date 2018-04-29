using FluentAssertions;
using NUnit.Framework;

namespace DataDog.Tracing.Tests
{
    [TestFixture]
    public class RootSpanTests
    {
        [Test]
        public void RootSpan_should_add_all_child_spans_to_list()
        {
            var root = new RootSpan();
            var first = root.Begin("first", "first", "first", "first");
            var second = first.Begin("second", "second", "second", "second");
            root.Spans.Count.Should().Be(3);
            root.Spans[0].Should().Be(root);
            root.Spans[1].Should().Be(first);
            root.Spans[2].Should().Be(second);
            second.Dispose();
            root.Spans[0].Should().Be(root);
            root.Spans[1].Should().Be(first);
            root.Spans[2].Should().Be(second);
            first.Dispose();
            root.Spans[0].Should().Be(root);
            root.Spans[1].Should().Be(first);
            root.Spans[2].Should().Be(second);
            root.Dispose();
            root.Spans[0].Should().Be(root);
            root.Spans[1].Should().Be(first);
            root.Spans[2].Should().Be(second);
        }
    }
}
