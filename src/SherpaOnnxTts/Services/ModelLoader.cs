using CloudyWing.SherpaOnnxTts.Configuration;
using CloudyWing.SherpaOnnxTts.Models;
using CloudyWing.SherpaOnnxTts.Services.Configurators;
using Microsoft.Extensions.Options;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Services;

/// <summary>
/// Service for loading TTS models.
/// </summary>
public partial class ModelLoader : IModelLoader {
    private readonly ILogger<ModelLoader> logger;
    private readonly TtsOptions options;
    private readonly Dictionary<ModelType, IModelConfigurator> configurators;

    public ModelLoader(
        ILogger<ModelLoader> logger,
        IOptions<TtsOptions> options,
        IEnumerable<IModelConfigurator> configurators
    ) {
        this.logger = logger;
        this.options = options.Value;
        this.configurators = configurators.ToDictionary(c => c.SupportedType);
    }

    /// <inheritdoc/>
    public ModelInfo? LoadModel(string modelDirectory, string modelName) {
        try {
            ModelType modelType = DetectModelType(modelDirectory);
            if (modelType == ModelType.Unknown) {
                LogUnknownModelStructure(modelDirectory);
                return null;
            }

            if (!configurators.TryGetValue(modelType, out IModelConfigurator? configurator)) {
                logger.LogError("No configurator found for model type {ModelType}", modelType);
                return null;
            }

            OfflineTtsConfig config = CreateBaseConfig();
            configurator.Configure(ref config, modelDirectory);

            OfflineTts tts = new(config);

            LogModelLoadSuccess(modelType, modelName);

            return new ModelInfo {
                Name = modelName,
                Directory = modelDirectory,
                Type = modelType,
                Instance = tts
            };
        } catch (Exception ex) {
            LogModelLoadFailure(ex, modelName, modelDirectory);
            return null;
        }
    }

    /// <inheritdoc/>
    public ModelType DetectModelType(string modelDirectory) {
        if (File.Exists(Path.Combine(modelDirectory, "model.onnx"))
            && File.Exists(Path.Combine(modelDirectory, "voices.bin"))
        ) {
            return ModelType.Kokoro;
        }

        if (File.Exists(Path.Combine(modelDirectory, "model-steps-3.onnx"))) {
            return ModelType.Matcha;
        }

        string[] onnxFiles = Directory.GetFiles(modelDirectory, "*.onnx");
        bool hasVitsModel = onnxFiles.Any(
            filePath => !filePath.Contains("vocos", StringComparison.OrdinalIgnoreCase)
                && !filePath.Contains("model-steps", StringComparison.OrdinalIgnoreCase)
        );

        if (hasVitsModel) {
            return ModelType.Vits;
        }

        return ModelType.Unknown;
    }

    private OfflineTtsConfig CreateBaseConfig() {
        return new OfflineTtsConfig {
            Model = {
                NumThreads = options.NumThreads,
                Debug = options.EnableDebug ? 1 : 0
            },
            MaxNumSentences = options.MaxNumSentences
        };
    }

    /// <summary>
    /// Logs a warning when an unknown model structure is detected in a directory.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown model structure in {Directory}")]
    private partial void LogUnknownModelStructure(string directory);

    /// <summary>
    /// Logs information when a model is successfully loaded.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Successfully loaded {Type} model: {Name}")]
    private partial void LogModelLoadSuccess(ModelType type, string name);

    /// <summary>
    /// Logs an error when a model fails to load.
    /// </summary>
    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to load model {Name} from {Directory}")]
    private partial void LogModelLoadFailure(Exception ex, string name, string directory);
}

