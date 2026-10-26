using System.Threading.RateLimiting;
using FluentValidation;
using HammadLatifLab.ChatBot.Api.Middleware;
using HammadLatifLab.ChatBot.Application.Validators;
using HammadLatifLab.ChatBot.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hammad Latif Lab AI ChatBot & FAQ API",
        Version = "v1",
        Description = "Grounded Medical Diagnostic Laboratory AI Assistant API powered by .NET 8 (C# 12), Clean Architecture, and Repository Pattern."
    });
});

// 2. Health Checks
builder.Services.AddHealthChecks();

// 3. Validators
builder.Services.AddValidatorsFromAssemblyContaining<ChatRequestValidator>();

// 4. Infrastructure & Application Layers (Repository Pattern & Services)
var faqFilePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "faqs.json");
var testsFilePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "tests.json");
builder.Services.AddInfrastructure(builder.Configuration, faqFilePath, testsFilePath);

// 5. CORS policy for React & Angular frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:3000", 
                "http://localhost:4300")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 6. Rate Limiting (Fixed window: 30 requests per minute per IP)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("chat_limit", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// 7. Configure Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hammad Latif Lab API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseRateLimiter();

app.UseAuthorization();

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
