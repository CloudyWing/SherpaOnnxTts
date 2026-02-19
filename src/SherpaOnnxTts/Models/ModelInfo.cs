using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Models;

/// <summary>
/// Information about a loaded TTS model.
/// </summary>
public class ModelInfo {
    /// <summary>
    /// The model name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The directory containing the model files.
    /// </summary>
    public required string Directory { get; init; }

    /// <summary>
    /// The type of the model.
    /// </summary>
    public required ModelType Type { get; init; }

    /// <summary>
    /// The loaded TTS instance.
    /// </summary>
    public required OfflineTts Instance { get; init; }

    /// <summary>
    /// The sample rate of the model.
    /// </summary>
    public int SampleRate => Instance.SampleRate;

    /// <summary>
    /// The number of speakers in the model.
    /// </summary>
    public int NumSpeakers => Instance.NumSpeakers;
}
