using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing
{
    public interface ISpan : IDisposable
    {
        ISpan Begin(string name, string resource, string type);
        void SetMeta(string name, string value);
        void SetError(Exception ex);
    }
}
