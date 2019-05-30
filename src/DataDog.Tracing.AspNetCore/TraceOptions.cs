using System;
using System.Collections.Generic;
using System.Text;

namespace DataDog.Tracing.AspNetCore
{
    public class TraceOptions
    {
        public string ServiceName { get; set; } = "web";
        public bool AnalyticsEnabled { get; set; }
    }
}
