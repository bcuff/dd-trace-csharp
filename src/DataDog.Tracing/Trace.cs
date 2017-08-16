using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    /// <summary>
    /// Encapsulates a completed trace.
    /// </summary>
    public class Trace
    {
        internal Trace(RootSpan root)
        {
            Root = root;
        }

        public string Name => Root.Name;

        public string ServiceName => Root.Service;

        public string Resource => Root.Resource;

        public string Type => Root.Type;

        public bool HasError => Root.Error != 0;

        public TimeSpan Duration => Util.FromNanoseconds(Root.Duration);

        internal RootSpan Root { get; }
    }
}
