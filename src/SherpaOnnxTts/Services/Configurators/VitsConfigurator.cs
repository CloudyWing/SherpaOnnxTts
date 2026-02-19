using CloudyWing.SherpaOnnxTts.Models;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Services.Configurators;

/// <summary>
/// Configurator for VITS TTS models.
/// </summary>
public partial class VitsConfigurator : IModelConfigurator {
    private readonly ILogger<VitsConfigurator> logger;

    public VitsConfigurator(ILogger<VitsConfigurator> logger) {
        this.logger = logger;
    }

    public ModelType SupportedType => ModelType.Vits;

    public void Configure(ref OfflineTtsConfig config, string modelDirectory) {
        LogConfiguringVitsModel(modelDirectory);

        string[] onnxFiles = Directory.GetFiles(modelDirectory, "*.onnx");
        string? vitsModel = onnxFiles.FirstOrDefault(filePath =>
            !filePath.Contains("vocos", StringComparison.OrdinalIgnoreCase)
            && !filePath.Contains("model-steps", StringComparison.OrdinalIgnoreCase)
        ) ?? throw new FileNotFoundException("VITS model file not found in directory", modelDirectory);

        string lexiconPath = Path.Combine(modelDirectory, "lexicon.txt");
        string tokensPath = Path.Combine(modelDirectory, "tokens.txt");
        string dataDirPath = Path.Combine(modelDirectory, "espeak-ng-data");

        ValidateFileExists(lexiconPath);
        ValidateFileExists(tokensPath);

        config.Model.Vits = new OfflineTtsVitsModelConfig {
            Model = vitsModel,
            Lexicon = lexiconPath,
            Tokens = tokensPath
        };

        if (Directory.Exists(dataDirPath)) {
            config.Model.Vits.DataDir = dataDirPath;
        }
    }

    private void ValidateFileExists(string path) {
        if (!File.Exists(path)) {
            LogMissingFile(path);
            throw new FileNotFoundException($"Required model file not found: {path}", path);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Configuring VITS model from {Directory}")]
    private partial void LogConfiguringVitsModel(string directory);

    [LoggerMessage(Level = LogLevel.Error, Message = "Required file not found: {Path}")]
    private partial void LogMissingFile(string path);
}
