using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace DataDog.Tracing.AspNetCore
{
    public static class DataDogTracingApplicationBuilderExtensions
    {

        public static IApplicationBuilder UseDataDogTracing(this IApplicationBuilder app, TraceSource source, string serviceName = "web")
            => app.UseDataDogTracing(source, new TraceOptions { ServiceName = serviceName });

        public static IApplicationBuilder UseDataDogTracing(this IApplicationBuilder app, TraceSource source, TraceOptions options) => app.Use(async (context, next) =>
        {
            var resource = context.Request.Host.Host;
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
            using (var span = source.Begin("aspnet.request", options.ServiceName, resource, "web"))
            using (var scope = new TraceContextScope(span))
            {
                if (options.AnalyticsEnabled)
                {
                    span.SetMeta("_dd1.sr.eausr", "true");
                }
                span.SetMeta("http.method", context.Request.Method);
                span.SetMeta("http.path", path);
                span.SetMeta("http.query", context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty);
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    span.SetError(ex);
                    throw;
                }
                span.SetMeta("http.status_code", context.Response.StatusCode.ToString());
            }
        });
    }
}
