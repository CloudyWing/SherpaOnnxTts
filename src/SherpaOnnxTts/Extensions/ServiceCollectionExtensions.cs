using CloudyWing.SherpaOnnxTts.Configuration;
using CloudyWing.SherpaOnnxTts.Services;
using CloudyWing.SherpaOnnxTts.Services.Configurators;

namespace CloudyWing.SherpaOnnxTts.Extensions;

/// <summary>
/// Extension methods for service collection configuration.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds TTS services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddTtsServices(this IServiceCollection services, IConfiguration configuration) {
        services.Configure<TtsOptions>(configuration.GetSection(TtsOptions.SectionName));
        services.AddSingleton<ITtsService, TtsService>();
        services.AddSingleton<IModelLoader, ModelLoader>();

        // Register Model Configurators
        services.AddSingleton<IModelConfigurator, KokoroConfigurator>();
        services.AddSingleton<IModelConfigurator, MatchaConfigurator>();
        services.AddSingleton<IModelConfigurator, VitsConfigurator>();

        return services;
    }
}
