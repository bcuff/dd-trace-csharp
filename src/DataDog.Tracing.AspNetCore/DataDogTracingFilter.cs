using Microsoft.AspNetCore.Mvc.Filters;

namespace DataDog.Tracing.AspNetCore
{
    public class DataDogTracingFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var span = TraceContext.Current;
            if (span != null)
            {
                var routeValues = context.ActionDescriptor.RouteValues;
                routeValues.TryGetValue("action", out string action);
                routeValues.TryGetValue("controller", out string controller);
                action = action ?? "unknown";
                controller = controller ?? "unknown";
                span.Resource = $"{controller}.{action}";
            }
        }
    }
}
