namespace CloudyWing.SherpaOnnxTts.Models.Api;

/// <summary>
/// Parameters for TTS generation.
/// </summary>
public record TtsGenerationParameters {
    /// <summary>
    /// The text to synthesize.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// The voice/model name to use.
    /// </summary>
    public string? Voice { get; init; }

    /// <summary>
    /// The speech speed multiplier.
    /// </summary>
    public float Speed { get; init; } = 1.0F;

    /// <summary>
    /// The speaker ID for multi-speaker models.
    /// </summary>
    public int SpeakerId { get; init; } = 0;

    /// <summary>
    /// The output audio format.
    /// </summary>
    public string Format { get; init; } = "wav";
}
