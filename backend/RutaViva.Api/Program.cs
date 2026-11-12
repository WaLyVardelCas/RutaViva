using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RutaViva.Api;
using RutaViva.Infrastructure;
using RutaViva.Job;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
    {
        string detail = string.Join(" ", context.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "El cuerpo de la solicitud no es válido."
                : error.ErrorMessage));
        return new BadRequestObjectResult(new ApiError("Datos inválidos", detail));
    });
builder.Services.AddRutaVivaOpenApi();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRutaViva();
builder.Services.AddHostedService<DailyEvaluationJob>();

WebApplication app = builder.Build();

app.UseRutaVivaExceptions();
app.UseCors();
app.UseRutaVivaOpenApi();
app.MapControllers();
app.Run();

public partial class Program;
