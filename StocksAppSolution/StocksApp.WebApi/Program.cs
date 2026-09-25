using StocksApp.Core.IoC;
using StocksApp.Infrastructure.IoC;
using StocksApp.Observability;
using StocksApp.WebApi.Hubs;
using StocksApp.WebApi.IoC;
using StocksApp.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiFramework(builder.Configuration)
    .AddIdentityCore(builder.Configuration)
    .AddStockCore()
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddInfrastructureMessaging(builder.Configuration)
    .AddPriceFeedSubscriber(builder.Configuration)
    .AddPriceFeedNotifications();


if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddObservability(builder.Configuration)
        .AddInfrastructureHealthChecks(builder.Configuration);
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandlingMiddleware();

//app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<StocksHub>("api/stocksHub");

app.MapHealthChecks("/health");

//Add Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API");
        c.RoutePrefix = "swagger"; 
    });
}

app.Run();

public partial class Program { } 