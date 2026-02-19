using System.Text;
using CloudyWing.SherpaOnnxTts.Utils;

namespace CloudyWing.SherpaOnnxTts.Tests.Util;

[TestFixture]
public class WaveUtilsTests {
    [Test]
    public void Encode_SimpleSineWave_CreatesValidWavHeader() {
        int sampleRate = 16000;
        // Small sample buffer
        float[] samples = new float[100];

        byte[] wavBytes = WaveUtils.Encode(sampleRate, samples);

        string riff = Encoding.ASCII.GetString(wavBytes, 0, 4);
        Assert.That(riff, Is.EqualTo("RIFF"));

        string wave = Encoding.ASCII.GetString(wavBytes, 8, 4);
        Assert.That(wave, Is.EqualTo("WAVE"));

        string fmt = Encoding.ASCII.GetString(wavBytes, 12, 4);
        Assert.That(fmt, Is.EqualTo("fmt "));

        short format = BitConverter.ToInt16(wavBytes, 20);
        Assert.That(format, Is.EqualTo(1));

        short channels = BitConverter.ToInt16(wavBytes, 22);
        Assert.That(channels, Is.EqualTo(1));

        int rate = BitConverter.ToInt32(wavBytes, 24);
        Assert.That(rate, Is.EqualTo(sampleRate));

        short bits = BitConverter.ToInt16(wavBytes, 34);
        Assert.That(bits, Is.EqualTo(16));

        string data = Encoding.ASCII.GetString(wavBytes, 36, 4);
        Assert.That(data, Is.EqualTo("data"));

        int dataSize = BitConverter.ToInt32(wavBytes, 40);
        Assert.That(dataSize, Is.EqualTo(samples.Length * 2));
    }
}
