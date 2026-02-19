using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using CloudyWing.SherpaOnnxTts.Configuration;
using CloudyWing.SherpaOnnxTts.Models;
using CloudyWing.SherpaOnnxTts.Models.Api;
using Microsoft.Extensions.Options;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Services;

/// <summary>
/// Service for text-to-speech operations.
/// </summary>
public partial class TtsService : ITtsService {
    private readonly ILogger<TtsService> logger;
    private readonly IModelLoader modelLoader;
    private readonly TtsOptions options;
    private readonly ConcurrentDictionary<string, ModelInfo> loadedModels = new(StringComparer.OrdinalIgnoreCase);
    private string defaultVoice;

    public TtsService(
        ILogger<TtsService> logger,
        IModelLoader modelLoader,
        IOptions<TtsOptions> options
    ) {
        this.logger = logger;
        this.modelLoader = modelLoader;
        this.options = options.Value;
        defaultVoice = this.options.DefaultVoice;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await Task.Run(LoadModels, cancellationToken);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<byte[]> GenerateAudioStream(TtsGenerationParameters parameters) {
        ModelInfo? modelInfo = GetModelInfo(parameters.Voice);
        if (modelInfo is null) {
            LogNoModelAvailable(parameters.Voice ?? "default");
            yield break;
        }

        Channel<byte[]> channel = Channel.CreateUnbounded<byte[]>();

        int NativeCallback(IntPtr samples, int n) {
            float[] floatData = new float[n];
            Marshal.Copy(samples, floatData, 0, n);

            // Directly send Float32 bytes (4 bytes per sample) to avoid quantization and format mismatch
            // Clients (Web Audio API, PyAudio paFloat32) handle this natively.
            byte[] bytes = new byte[n * 4];
            Buffer.BlockCopy(floatData, 0, bytes, 0, bytes.Length);

            channel.Writer.TryWrite(bytes);
            return 1;
        }

        // Keep reference to wrapper to prevent GC
        OfflineTtsCallback wrapper = new(NativeCallback);

        _ = Task.Run(() => {
            try {
                modelInfo.Instance.GenerateWithCallback(
                    parameters.Text,
                    parameters.Speed,
                    parameters.SpeakerId,
                    wrapper
                );
            } catch (Exception ex) {
                LogAudioGenerationFailure(ex, modelInfo.Name);
            } finally {
                channel.Writer.Complete();
            }
        });

        await foreach (byte[] chunk in channel.Reader.ReadAllAsync()) {
            yield return chunk;
        }
    }

    /// <inheritdoc/>
    public GeneratedAudio? GenerateAudio(TtsGenerationParameters parameters) {
        ModelInfo? modelInfo = GetModelInfo(parameters.Voice);
        if (modelInfo is null) {
            LogNoModelAvailable(parameters.Voice ?? "default");
            return null;
        }

        try {
            dynamic result = modelInfo.Instance.Generate(parameters.Text, parameters.SpeakerId, (int)parameters.Speed);
            return new GeneratedAudio(result.SampleRate, result.Samples);
        } catch (Exception ex) {
            LogAudioGenerationFailure(ex, modelInfo.Name);
            return null;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<ModelInfo> GetLoadedModels() {
        return loadedModels.Values.ToList();
    }

    /// <inheritdoc/>
    public ModelInfo? GetModelInfo(string? modelName = null) {
        if (string.IsNullOrWhiteSpace(modelName)) {
            modelName = defaultVoice;
        }

        if (loadedModels.TryGetValue(modelName, out ModelInfo? model)) {
            return model;
        }

        if (loadedModels.TryGetValue(defaultVoice, out ModelInfo? defaultModel)) {
            LogModelNotFoundFallback(modelName, defaultVoice);
            return defaultModel;
        }

        return null;
    }

    /// <inheritdoc/>
    public string GetDefaultVoice() => defaultVoice;

    /// <inheritdoc/>
    public int GetModelSampleRate(string? modelName = null) {
        ModelInfo? info = GetModelInfo(modelName);
        return info?.SampleRate ?? 22050; // Fallback to common default if something weird happens
    }

    private void LoadModels() {
        if (!Directory.Exists(options.ModelsDirectory)) {
            LogModelsDirectoryNotFound(options.ModelsDirectory);
            return;
        }

        string[] modelDirectories = Directory.GetDirectories(options.ModelsDirectory);
        LogScanningForModels(modelDirectories.Length);

        foreach (string directory in modelDirectories) {
            string? modelName = Path.GetFileName(directory);
            if (string.IsNullOrEmpty(modelName)) {
                continue;
            }

            ModelInfo? modelInfo = modelLoader.LoadModel(directory, modelName);
            if (modelInfo is not null) {
                loadedModels[modelName] = modelInfo;
            }
        }

        UpdateDefaultVoice();
        LogLoadedModelsDetails();
    }

    private void UpdateDefaultVoice() {
        if (loadedModels.IsEmpty) {
            LogNoModelsLoaded();
            return;
        }

        if (!loadedModels.ContainsKey(defaultVoice)) {
            defaultVoice = loadedModels.Keys.First();
            LogDefaultVoiceChanged(defaultVoice);
        } else {
            LogDefaultVoiceSet(defaultVoice);
        }
    }

    private void LogLoadedModelsDetails() {
        if (loadedModels.IsEmpty) {
            return;
        }

        LogModelsLoadedCount(loadedModels.Count);
        foreach (KeyValuePair<string, ModelInfo> modelEntry in loadedModels) {
            LogModelDetails(modelEntry.Key, modelEntry.Value.Type, modelEntry.Value.SampleRate);
        }
    }

    /// <summary>
    /// Logs a warning when no model is available for the requested voice.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "No model available for voice: {Voice}")]
    partial void LogNoModelAvailable(string voice);

    /// <summary>
    /// Logs an error when audio generation fails.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to generate audio using model {Model}")]
    partial void LogAudioGenerationFailure(Exception ex, string model);

    /// <summary>
    /// Logs a warning when a requested model is not found and a fallback is used.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Model {Model} not found, falling back to {Default}")]
    partial void LogModelNotFoundFallback(string model, string @default);

    /// <summary>
    /// Logs a warning when the models directory does not exist.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Models directory {Directory} does not exist")]
    partial void LogModelsDirectoryNotFound(string directory);

    /// <summary>
    /// Logs information about scanning directories for models.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Scanning {Count} directories for TTS models")]
    partial void LogScanningForModels(int count);

    /// <summary>
    /// Logs a warning when no models are loaded.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "No TTS models were loaded")]
    partial void LogNoModelsLoaded();

    /// <summary>
    /// Logs information when the default voice is changed because the configured one was not found.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Configured default voice not found, using: {Voice}")]
    partial void LogDefaultVoiceChanged(string voice);

    /// <summary>
    /// Logs information when the default voice is successfully set.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Default voice set to: {Voice}")]
    partial void LogDefaultVoiceSet(string voice);

    /// <summary>
    /// Logs the count of successfully loaded models.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully loaded {Count} TTS model(s):")]
    partial void LogModelsLoadedCount(int count);

    /// <summary>
    /// Logs detailed information about a loaded model.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "  - {Name} ({Type}, {SampleRate}Hz)")]
    partial void LogModelDetails(string name, ModelType type, int sampleRate);
}
