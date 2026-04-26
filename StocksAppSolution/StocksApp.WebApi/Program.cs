using StocksApp.Core;
using StocksApp.Infrastructure;
using StocksApp.WebApi.Middlewares;
using StocksApp.WebApi.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.Configure<TradeOptions>(builder.Configuration.GetSection("TradingOptions"));

builder.Services.AddCore();

builder.Services.AddInfrastructure(builder.Configuration);

// Configure Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionsHandlingMiddleware();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//Add Swagger middleware
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API");
    c.RoutePrefix = "swagger"; 
});

app.Run();

public partial class Program { } // Makes this class accessible for integration testing purposes.