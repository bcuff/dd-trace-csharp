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
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
            var name = $"{context.Request.Method} {path}";
            using (var span = source.Begin(path, serviceName, resource, "web"))
            using (var scope = new TraceContextScope(span))
            {
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
