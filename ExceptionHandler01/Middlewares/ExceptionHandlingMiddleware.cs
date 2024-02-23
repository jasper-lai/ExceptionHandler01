namespace ExceptionHandler01.Middlewares
{
    using Microsoft.AspNetCore.Mvc;
    using System.Net;
    using System.Text.Json;
    using System.Threading;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var routeData = context.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();
            var actionName = routeData?.Values["action"]?.ToString();
            _logger.LogInformation("Controller={controller} Action={action}", controllerName, actionName);

            var response = new ProblemDetails()
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = exception.Message,
                Instance = context.Request.Path,
            };

            // 
            await context.Response
                .WriteAsJsonAsync(response);
        }

    }
}
