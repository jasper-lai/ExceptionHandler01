
## Description

this is a example for testing ASP.NET Core 8 new feature: IExceptionHandler.  

## Major Issues

**I have the following code, and the context.GetRouteData() always return null.**  
**Is it the bug of IExceptionHandler and app.UseExceptionHandler(); ?**

I want to log the controller name and action name when exception occurs.

I’ve found the Program.cs call UseExceptionHandler() twice:  
(1) app.UseExceptionHandler("/Home/Error");  
(2) app.UseExceptionHandler();  

But even comment the first app.UseExceptionHandler("/Home/Error"); , still cannot get the routeData.  

I've tried the custom middleware (ExceptionHandlingMiddleware.cs) to catch exception, it can get the routeData.  

the related code as follows:  

**GlobalExceptionHandler.cs**   

```json
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
        // !!! the routeData is always null  !!!
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
```

**Program.cs**   
```json
using ExceptionHandler01.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region (jasper) register GlobalExceptionHandler and ProblemDeatils to DI
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//// (jasper) use custom exception handler
//app.UseMiddleware<ExceptionHandlingMiddleware>();

// (jasper) use default exception handler
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

## Refreence

* <a href="https://www.milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-8" target="_blank">(Milan Jovanovic) Global Error Handling in ASP.NET Core 8</a>  
> the post show how to use the new feature for global error handling with IExceptionHandler and asp.UseExceptionHandler.  

* <a href="https://stackoverflow.com/questions/78039198/cannot-get-routedata-in-iexceptionhandler-tryhandleasync-for-asp-net-core-8" target="_blank">(jasper)(stakoverflow) Cannot get routeData in IExceptionHandler.TryHandleAsync() for ASP.NET Core 8</a>  
> this is my post on stackoverflow.  



