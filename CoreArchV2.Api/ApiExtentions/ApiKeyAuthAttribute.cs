namespace CoreArchV2.Api.ApiExtentions
{
    using CoreArchV2.Api.Helper;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Net;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderApiKey = "ApiKey";
        private const string HeaderApiSecret = "ApiSecret";

        private const string ValidKey = "bsrn@-123";
        private const string ValidSecret = "x8ZbT723sKq";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;

            // Header kontrolü
            if (!request.Headers.TryGetValue(HeaderApiKey, out var key) ||
                !request.Headers.TryGetValue(HeaderApiSecret, out var secret) ||
                key != ValidKey || secret != ValidSecret)
            {
                context.Result = new ObjectActionResult(
                    success: false,
                    statusCode: HttpStatusCode.Unauthorized,
                    data: null);
                return;
            }

            await next();
        }
    }

}
