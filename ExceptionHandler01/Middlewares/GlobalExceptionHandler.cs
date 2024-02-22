namespace ExceptionHandler01.Middlewares
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            var routeData = context.GetRouteData();
            var controllerName = routeData?.Values["controller"]?.ToString();
            var actionName = routeData?.Values["action"]?.ToString();

            _logger.LogInformation("Controller={controller} Action={action}", controllerName, actionName);

            var response = new ProblemDetails()
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = "occurs Internal Server Error, please contact MIS",
                Instance = context.Request.Path,
            };

            // 
            await context.Response
                .WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
