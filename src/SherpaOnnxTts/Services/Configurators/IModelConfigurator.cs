using CloudyWing.SherpaOnnxTts.Models;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Services.Configurators;

/// <summary>
/// Interface for configuring offline TTS models.
/// </summary>
public interface IModelConfigurator {
    /// <summary>
    /// Gets the model type supported by this configurator.
    /// </summary>
    ModelType SupportedType { get; }

    /// <summary>
    /// Configures the offline TTS settings for the specified model directory.
    /// </summary>
    /// <param name="config">The configuration object to update.</param>
    /// <param name="modelDirectory">The directory containing the model files.</param>
    void Configure(ref OfflineTtsConfig config, string modelDirectory);
}
