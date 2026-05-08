using StocksApp.Core;
using StocksApp.Infrastructure;
using StocksApp.WebApi;
using StocksApp.WebApi.Hubs;
using StocksApp.WebApi.Options;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddWebApi(builder.Configuration)
                .AddCore(builder.Configuration)
                .AddInfrastructure(builder.Configuration);

builder.Services.Configure<TradeOptions>(builder.Configuration.GetSection("TradingOptions"));

// Configure Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<StocksHub>("stocksHub");

//Add Swagger middleware
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API");
    c.RoutePrefix = "swagger"; 
});

app.Run();

public partial class Program { } // For integration testing purposes