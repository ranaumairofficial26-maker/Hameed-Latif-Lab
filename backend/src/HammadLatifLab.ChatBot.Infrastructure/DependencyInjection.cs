using HammadLatifLab.ChatBot.Application.Configuration;
using HammadLatifLab.ChatBot.Application.Interfaces;
using HammadLatifLab.ChatBot.Application.Services;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using HammadLatifLab.ChatBot.Infrastructure.Clients;
using HammadLatifLab.ChatBot.Infrastructure.Repositories;
using HammadLatifLab.ChatBot.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HammadLatifLab.ChatBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        string faqFilePath,
        string testsFilePath)
    {
        // 1. Bind Options (Supports appsettings.json, appsettings.Development.json, and GEMINI_API_KEY env var)
        services.Configure<GeminiOptions>(options =>
        {
            configuration.GetSection(GeminiOptions.SectionName).Bind(options);

            var envApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                            ?? Environment.GetEnvironmentVariable("Gemini__ApiKey");

            if (!string.IsNullOrWhiteSpace(envApiKey))
            {
                options.ApiKey = envApiKey.Trim();
            }
        });

        // 2. Repositories (Repository Pattern)
        services.AddSingleton<IFaqRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JsonFaqRepository>>();
            return new JsonFaqRepository(faqFilePath, logger);
        });

        services.AddSingleton<ITestRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JsonTestRepository>>();
            return new JsonTestRepository(testsFilePath, logger);
        });

        services.AddSingleton<IChatLogRepository, InMemoryChatLogRepository>();
        services.AddSingleton<IChatSessionStore, InMemoryChatSessionStore>();

        // 3. Application Services
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<IChatService, ChatService>();

        // 4. HTTP Client with Resilience
        var geminiConfig = configuration.GetSection(GeminiOptions.SectionName).Get<GeminiOptions>() ?? new GeminiOptions();
        
        services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
        {
            var baseUrl = geminiConfig.BaseUrl.TrimEnd('/') + "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(6);
        });

        return services;
    }
}
