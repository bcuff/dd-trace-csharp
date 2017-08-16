using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    /// <summary>
    /// Encapsulates an APM trace or a span within a trace.
    /// </summary>
    public interface ISpan : IDisposable
    {
        /// <summary>
        /// Begins a new span.
        /// </summary>
        /// <param name="name">
        /// The name of the span. This will be the title of the span when viewing the trace.
        /// </param>
        /// <param name="serviceName">This is the name of the service. e.g. my_appication, memcached, dynamodb, etc.</param>
        /// <param name="resource">The underlying resource. e.g. /home for web or GET for memcached. This shouldn't have too many unique combinations.</param>
        /// <param name="type">The category of the service. Typically web, db, or cache.</param>
        /// <returns>The new span.</returns>
        ISpan Begin(string name, string serviceName, string resource, string type);
        /// <summary>
        /// Sets metadata on the span. This is any additional information you might want to see when looking at the trace.
        /// e.g. keys or queries.
        /// </summary>
        /// <param name="name">The key of the metadata tag.</param>
        /// <param name="value">The value.</param>
        void SetMeta(string name, string value);
        /// <summary>
        /// Sets an error on the span along with the message, stack, and exception type.
        /// </summary>
        /// <param name="ex">The exception.</param>
        void SetError(Exception ex);
    }
}
