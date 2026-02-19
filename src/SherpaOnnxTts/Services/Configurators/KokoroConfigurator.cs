using CloudyWing.SherpaOnnxTts.Models;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Services.Configurators;

/// <summary>
/// Configurator for Kokoro TTS models.
/// </summary>
public partial class KokoroConfigurator : IModelConfigurator {
    private readonly ILogger<KokoroConfigurator> logger;

    public KokoroConfigurator(ILogger<KokoroConfigurator> logger) {
        this.logger = logger;
    }

    public ModelType SupportedType => ModelType.Kokoro;

    public void Configure(ref OfflineTtsConfig config, string modelDirectory) {
        LogConfiguringKokoroModel(modelDirectory);

        string modelPath = Path.Combine(modelDirectory, "model.onnx");
        string voicesPath = Path.Combine(modelDirectory, "voices.bin");
        string tokensPath = Path.Combine(modelDirectory, "tokens.txt");
        string dataDirPath = Path.Combine(modelDirectory, "espeak-ng-data");

        ValidateFileExists(modelPath);
        ValidateFileExists(voicesPath);
        ValidateFileExists(tokensPath);

        if (!Directory.Exists(dataDirPath)) {
            LogMissingDirectory(dataDirPath);
        }

        OfflineTtsKokoroModelConfig kokoro = new() {
            Model = modelPath,
            Voices = voicesPath,
            Tokens = tokensPath,
            DataDir = dataDirPath
        };

        string[] lexicons = Directory.GetFiles(modelDirectory, "lexicon*.txt");
        if (lexicons.Length > 0) {
            kokoro.Lexicon = string.Join(",", lexicons);
        }

        config.Model.Kokoro = kokoro;
    }

    private void ValidateFileExists(string path) {
        if (!File.Exists(path)) {
            LogMissingFile(path);
            throw new FileNotFoundException($"Required model file not found: {path}", path);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Configuring Kokoro model from {Directory}")]
    private partial void LogConfiguringKokoroModel(string directory);

    [LoggerMessage(Level = LogLevel.Error, Message = "Required file not found: {Path}")]
    private partial void LogMissingFile(string path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Directory not found: {Path}")]
    private partial void LogMissingDirectory(string path);
}
