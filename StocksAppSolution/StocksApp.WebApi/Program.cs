using StocksApp.Core;
using StocksApp.Infrastructure.IoC;
using StocksApp.Observability;
using StocksApp.WebApi.Hubs;
using StocksApp.WebApi.IoC;
using StocksApp.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi(builder.Configuration)
                .AddCore(builder.Configuration)
                .AddInfrastructure(builder.Configuration, builder.Environment)
                .AddRabbitMqCommandSender(builder.Configuration);

if(!builder.Environment.IsEnvironment("Test"))
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

app.MapHub<StocksHub>("stocksHub");

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