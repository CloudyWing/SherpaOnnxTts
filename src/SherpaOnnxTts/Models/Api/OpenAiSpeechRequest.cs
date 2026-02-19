using System.Text.Json.Serialization;

namespace CloudyWing.SherpaOnnxTts.Models.Api;

/// <summary>
/// Request model for OpenAI-compatible speech endpoint.
/// </summary>
public record OpenAiSpeechRequest {
    /// <summary>
    /// The model to use for synthesis.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>
    /// The text to synthesize.
    /// </summary>
    [JsonPropertyName("input")]
    public string? Input { get; init; }

    /// <summary>
    /// The voice to use for synthesis.
    /// </summary>
    [JsonPropertyName("voice")]
    public string? Voice { get; init; }

    /// <summary>
    /// The output audio format (only pcm).
    /// </summary>
    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; init; }

    /// <summary>
    /// The speech speed multiplier.
    /// </summary>
    [JsonPropertyName("speed")]
    public float? Speed { get; init; }
}
