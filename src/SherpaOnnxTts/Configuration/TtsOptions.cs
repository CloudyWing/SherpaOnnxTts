namespace CloudyWing.SherpaOnnxTts.Configuration;

/// <summary>
/// Configuration options for the TTS service.
/// </summary>
public class TtsOptions {
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Tts";

    /// <summary>
    /// Directory containing TTS models.
    /// </summary>
    public string ModelsDirectory { get; set; } = "/app/models";

    /// <summary>
    /// Default voice/model name to use.
    /// </summary>
    public string DefaultVoice { get; set; } = "matcha-icefall-zh-en";

    /// <summary>
    /// Number of threads to use for model processing.
    /// </summary>
    public int NumThreads { get; set; } = 2;

    /// <summary>
    /// Enable debug mode for models.
    /// </summary>
    public bool EnableDebug { get; set; } = false;

    /// <summary>
    /// Maximum number of sentences to process at once.
    /// </summary>
    public int MaxNumSentences { get; set; } = 1;
}
