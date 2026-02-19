using CloudyWing.SherpaOnnxTts.Models;
using CloudyWing.SherpaOnnxTts.Models.Api;

namespace CloudyWing.SherpaOnnxTts.Services;

/// <summary>
/// Interface for TTS service operations.
/// </summary>
public interface ITtsService {
    /// <summary>
    /// Initializes the TTS service and loads all available models.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates audio from text using specified parameters.
    /// </summary>
    GeneratedAudio? GenerateAudio(TtsGenerationParameters parameters);

    /// <summary>
    /// Generates audio stream from text using specified parameters.
    /// </summary>
    /// <summary>
    /// Generates audio stream from text using specified parameters.
    /// </summary>
    IAsyncEnumerable<byte[]> GenerateAudioStream(TtsGenerationParameters parameters);

    /// <summary>
    /// Gets the list of loaded models with details.
    /// </summary>
    IReadOnlyList<ModelInfo> GetLoadedModels();

    /// <summary>
    /// Gets information about a specific model.
    /// </summary>
    ModelInfo? GetModelInfo(string? modelName = null);

    /// <summary>
    /// Gets the default voice name.
    /// </summary>
    string GetDefaultVoice();

    /// <summary>
    /// Gets the sample rate for a specific model (or default if null).
    /// </summary>
    int GetModelSampleRate(string? modelName = null);
}
