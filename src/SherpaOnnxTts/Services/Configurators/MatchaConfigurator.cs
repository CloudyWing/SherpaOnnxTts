using CloudyWing.SherpaOnnxTts.Models;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Services.Configurators;

/// <summary>
/// Configurator for Matcha TTS models.
/// </summary>
public partial class MatchaConfigurator : IModelConfigurator {
    private readonly ILogger<MatchaConfigurator> logger;

    public MatchaConfigurator(ILogger<MatchaConfigurator> logger) {
        this.logger = logger;
    }

    public ModelType SupportedType => ModelType.Matcha;

    public void Configure(ref OfflineTtsConfig config, string modelDirectory) {
        LogConfiguringMatchaModel(modelDirectory);

        string modelPath = Path.Combine(modelDirectory, "model-steps-3.onnx");
        string vocoderPath = Path.Combine(modelDirectory, "vocos-16khz-univ.onnx");
        string lexiconPath = Path.Combine(modelDirectory, "lexicon.txt");
        string tokensPath = Path.Combine(modelDirectory, "tokens.txt");
        string dataDirPath = Path.Combine(modelDirectory, "espeak-ng-data");

        ValidateFileExists(modelPath);
        ValidateFileExists(vocoderPath);
        ValidateFileExists(lexiconPath);
        ValidateFileExists(tokensPath);

        config.Model.Matcha = new OfflineTtsMatchaModelConfig {
            AcousticModel = modelPath,
            Vocoder = vocoderPath,
            Lexicon = lexiconPath,
            Tokens = tokensPath,
            DataDir = dataDirPath
        };
    }

    private void ValidateFileExists(string path) {
        if (!File.Exists(path)) {
            LogMissingFile(path);
            throw new FileNotFoundException($"Required model file not found: {path}", path);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Configuring Matcha model from {Directory}")]
    private partial void LogConfiguringMatchaModel(string directory);

    [LoggerMessage(Level = LogLevel.Error, Message = "Required file not found: {Path}")]
    private partial void LogMissingFile(string path);
}
