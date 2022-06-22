using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ReactSpa_Backend.Authorization;
using ReactSpa_Backend.Entities;
using ReactSpa_Backend.Helpers;
using ReactSpa_Backend.IServices;
using ReactSpa_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var env = builder.Environment;

// Use postgres db in production and sqlite db in development
if (env.IsProduction())
    services.AddDbContext<DataContext>();
else
    services.AddDbContext<DataContext, SqliteDataContext>();

services.AddCors();
services.AddControllers()
.AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Configure strongly typed settings objects.
services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//Configure DI for application services
services.AddScoped<IJwtUtils, JwtUtils>();
services.AddScoped<IAccountService, AccountService>();
services.AddScoped<IEmailService, EmailService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(builder =>
    builder
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

//Global error handling
app.UseMiddleware<ErrorHandlerMiddleware>();

//Custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run("http://localhost:4000");
