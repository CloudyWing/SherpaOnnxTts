namespace CloudyWing.SherpaOnnxTts.Utils;

/// <summary>
/// Audio format constants and utilities.
/// </summary>
public static class AudioFormatUtils {
    /// <summary>
    /// WAV audio format.
    /// </summary>
    public const string Wav = "wav";

    /// <summary>
    /// PCM audio format (raw samples).
    /// </summary>
    public const string Pcm = "pcm";

    /// <summary>
    /// Content type for WAV audio.
    /// </summary>
    public const string WavContentType = "audio/wav";

    /// <summary>
    /// Content type for PCM audio.
    /// </summary>
    public const string PcmContentType = "audio/pcm";

    /// <summary>
    /// Converts float samples to PCM bytes (16-bit).
    /// </summary>
    public static byte[] ToPcmBytes(float[] samples) {
        byte[] pcmBytes = new byte[samples.Length * sizeof(short)];

        for (int i = 0; i < samples.Length; i++) {
            short value = (short)(Math.Clamp(samples[i], -1.0f, 1.0f) * short.MaxValue);
            BitConverter.TryWriteBytes(pcmBytes.AsSpan(i * sizeof(short)), value);
        }

        return pcmBytes;
    }
}
