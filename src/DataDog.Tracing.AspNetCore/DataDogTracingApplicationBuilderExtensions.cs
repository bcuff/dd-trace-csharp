using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace DataDog.Tracing.AspNetCore
{
    public static class DataDogTracingApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDataDogTracing(this IApplicationBuilder app, TraceSource source, string serviceName = "web") => app.Use(async (context, next) =>
        {
            var resource = context.Request.Host.Host;
            using (var span = source.Begin(context.Request.Method, serviceName, resource, "web"))
            using (var scope = new TraceContextScope(span))
            {
                span.SetMeta("http.method", context.Request.Method);
                span.SetMeta("http.path", context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty);
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
