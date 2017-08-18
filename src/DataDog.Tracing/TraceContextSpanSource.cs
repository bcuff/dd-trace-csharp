using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    public class TraceContextSpanSource : ISpanSource
    {
        public static ISpanSource Instance { get; } = new TraceContextSpanSource();

        public ISpan Begin(string name, string serviceName, string resource, string type)
        {
            return TraceContext.Current?.Begin(name, serviceName, resource, type);
        }
    }
}
