using StocksApp.Core;
using StocksApp.Infrastructure;
using StocksApp.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StocksApp.WebApi.Hubs;
using StocksApp.WebApi.Middlewares;
using StocksApp.WebApi.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
    };
});


builder.Services.AddWebApi(builder.Configuration)
                .AddCore(builder.Configuration)
                .AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.Configure<TradeOptions>(builder.Configuration.GetSection("TradingOptions"));

// Configure Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandlingMiddleware();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<StocksHub>("stocksHub");

//Add Swagger middleware
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API");
        c.RoutePrefix = "swagger"; 
    });
}

app.Run();

public partial class Program { } // For integration testing purposes