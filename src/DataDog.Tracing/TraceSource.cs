using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace DataDog.Tracing
{
    /// <summary>
    /// For creating new traces.
    /// </summary>
    public class TraceSource : IObservable<Trace>, ISpanSource
    {
        readonly Subject<Trace> _subject = new Subject<Trace>();

        /// <summary>
        /// Begins a new trace.
        /// </summary>
        /// <param name="name">
        /// The name of the trace. This will be the title of the trace when viewing the trace.
        /// </param>
        /// <param name="serviceName">This is the name of the service. e.g. my_appication, memcached, dynamodb, etc.</param>
        /// <param name="resource">The underlying resource. e.g. /home for web or GET for memcached. This shouldn't have too many unique combinations.</param>
        /// <param name="type">The category of the service. Typically web, db, or cache.</param>
        /// <returns>The new trace.</returns>
       public ISpan Begin(string name, string serviceName, string resource, string type) => new RootSpan(_subject)
        {
            TraceId = Util.NewTraceId(),
            SpanId = Util.NewSpanId(),
            Name = name,
            Resource = resource,
            Type = type,
            Service = serviceName,
            Start = Util.GetTimestamp(),
        };

        /// <summary>
        /// Subscribes to traces that originate from this source instance when they complete.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns>An unsubscribe disposable.</returns>
        public IDisposable Subscribe(IObserver<Trace> observer)
        {
            return _subject.Subscribe(observer);
        }
    }
}
