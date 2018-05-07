namespace DataDog.Tracing
{
    public interface ISpanSource
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
    }
}
