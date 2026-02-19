using CloudyWing.SherpaOnnxTts.Utils;

namespace CloudyWing.SherpaOnnxTts.Tests.Util;

[TestFixture]
public class AudioFormatUtilsTests {
    [Test]
    public void ToPcmBytes_SilentAudio_ReturnsZeroBytes() {
        float[] samples = [0.0F, 0.0F, 0.0F];

        byte[] result = AudioFormatUtils.ToPcmBytes(samples);

        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Has.Length.EqualTo(samples.Length * 2));
            Assert.That(result, Is.All.EqualTo(0));
        }
    }

    [Test]
    public void ToPcmBytes_MaxVolume_ReturnsMaxShort() {
        float[] samples = [1.0F];

        byte[] result = AudioFormatUtils.ToPcmBytes(samples);

        short value = BitConverter.ToInt16(result, 0);
        Assert.That(value, Is.EqualTo(short.MaxValue));
    }

    [Test]
    public void ToPcmBytes_MinVolume_ReturnsMinShort() {
        float[] samples = [-1.0F];

        byte[] result = AudioFormatUtils.ToPcmBytes(samples);

        short value = BitConverter.ToInt16(result, 0);
        Assert.That(value, Is.EqualTo(-short.MaxValue));
    }

    [Test]
    public void ToPcmBytes_ClippingPositive_ClampsToMax() {
        float[] samples = [1.5F];

        byte[] result = AudioFormatUtils.ToPcmBytes(samples);

        short value = BitConverter.ToInt16(result, 0);
        Assert.That(value, Is.EqualTo(short.MaxValue));
    }

    [Test]
    public void ToPcmBytes_ClippingNegative_ClampsToMin() {
        float[] samples = [-1.5F];

        byte[] result = AudioFormatUtils.ToPcmBytes(samples);

        short value = BitConverter.ToInt16(result, 0);
        Assert.That(value, Is.EqualTo(-short.MaxValue));
    }
}
