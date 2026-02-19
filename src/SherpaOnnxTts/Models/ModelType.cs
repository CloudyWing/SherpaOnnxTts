namespace CloudyWing.SherpaOnnxTts.Models;

/// <summary>
/// Types of TTS models supported.
/// </summary>
public enum ModelType {
    /// <summary>
    /// Unknown or unsupported model type.
    /// </summary>
    Unknown,

    /// <summary>
    /// Kokoro TTS model.
    /// </summary>
    Kokoro,

    /// <summary>
    /// Matcha TTS model.
    /// </summary>
    Matcha,

    /// <summary>
    /// VITS TTS model.
    /// </summary>
    Vits
}
