using ExceptionHandler01.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//#region (jasper) register GlobalExceptionHandler and ProblemDeatils to DI
//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();
//#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(appError =>
    {
        appError.Run(async context => {
            var routeData = context.Features.Get<IRoutingFeature>()?.RouteData;
            if (routeData != null)
            {
                context.Items["controller"] = routeData.Values["controller"];
                context.Items["action"] = routeData.Values["action"];
            }

            var controllerName = context.Items["controller"];
            var actionName = context.Items["action"];

            var error = new { IsSuccess = false, Controller = controllerName, Action= actionName };
            await context.Response
                .WriteAsJsonAsync(error);
            //await Task.Run(() =>
            //{
            //   Console.WriteLine("");
            //});
        });
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// capture route data
app.Use(async (context, next) =>
{
    var routeData = context.GetRouteData();
    var controllerName = routeData?.Values["controller"]?.ToString();
    var actionName = routeData?.Values["action"]?.ToString();

    context.Items["controller"] = controllerName;
    context.Items["action"] = actionName;

    await next();
    // You can save route data in HttpContext.Items here
    
});

//// (jasper) use custom exception handler
//app.UseMiddleware<ExceptionHandlingMiddleware>();

//// (jasper) use default exception handler
//app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
