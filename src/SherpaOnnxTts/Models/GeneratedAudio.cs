namespace CloudyWing.SherpaOnnxTts.Models;

/// <summary>
/// Represents generated audio from TTS synthesis.
/// </summary>
public class GeneratedAudio {
    /// <summary>
    /// The sample rate in Hz.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// The audio samples as float array.
    /// </summary>
    public float[] Samples { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratedAudio"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <param name="samples">The audio samples as float array.</param>
    public GeneratedAudio(int sampleRate, float[] samples) {
        SampleRate = sampleRate;
        Samples = samples;
    }
}
