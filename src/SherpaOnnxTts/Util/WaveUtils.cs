namespace CloudyWing.SherpaOnnxTts.Utils;

/// <summary>
/// Utility class for encoding audio data to WAV format.
/// </summary>
public static class WaveUtils {
    private const string RiffHeader = "RIFF";
    private const string WaveHeader = "WAVE";
    private const string FmtHeader = "fmt ";
    private const string DataHeader = "data";

    private const short AudioFormat = 1;
    private const short NumChannels = 1;
    private const short BitsPerSample = 16;

    /// <summary>
    /// Encodes float audio samples to WAV format.
    /// </summary>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="samples">Audio samples as float array (-1.0 to 1.0)</param>
    /// <returns>WAV file as byte array</returns>
    public static byte[] Encode(int sampleRate, float[] samples) {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        short[] shortSamples = ConvertToInt16Samples(samples);
        int dataSize = shortSamples.Length * sizeof(short);

        WriteRiffHeader(writer, dataSize);
        WriteFormatChunk(writer, sampleRate);
        WriteDataChunk(writer, shortSamples);

        return memoryStream.ToArray();
    }

    private static short[] ConvertToInt16Samples(float[] samples) {
        short[] shortSamples = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++) {
            shortSamples[i] = (short)(Math.Clamp(samples[i], -1.0f, 1.0f) * short.MaxValue);
        }
        return shortSamples;
    }

    private static void WriteRiffHeader(BinaryWriter writer, int dataSize) {
        const int headerSize = 36;

        writer.Write(RiffHeader.ToCharArray());
        writer.Write(headerSize + dataSize);
        writer.Write(WaveHeader.ToCharArray());
    }

    private static void WriteFormatChunk(BinaryWriter writer, int sampleRate) {
        const int formatChunkSize = 16;
        short blockAlign = NumChannels * BitsPerSample / 8;
        int byteRate = sampleRate * blockAlign;

        writer.Write(FmtHeader.ToCharArray());
        writer.Write(formatChunkSize);
        writer.Write(AudioFormat);
        writer.Write(NumChannels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(BitsPerSample);
    }

    private static void WriteDataChunk(BinaryWriter writer, short[] samples) {
        writer.Write(DataHeader.ToCharArray());
        writer.Write(samples.Length * sizeof(short));

        foreach (short sample in samples) {
            writer.Write(sample);
        }
    }
}
