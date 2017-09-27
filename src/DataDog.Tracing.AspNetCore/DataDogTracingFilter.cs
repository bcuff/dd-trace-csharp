using System;
using System.Collections.Generic;
using System.Text;
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
                span.Resource = context.ActionDescriptor.DisplayName;
            }
        }
    }
}
