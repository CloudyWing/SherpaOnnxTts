using CloudyWing.SherpaOnnxTts.Configuration;
using CloudyWing.SherpaOnnxTts.Models;
using CloudyWing.SherpaOnnxTts.Services;
using CloudyWing.SherpaOnnxTts.Services.Configurators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SherpaOnnx;

namespace CloudyWing.SherpaOnnxTts.Tests.Services;

[TestFixture]
public class ModelLoaderTests {
    private ILogger<ModelLoader> logger;
    private IModelConfigurator kokoroConfigurator;
    private IModelConfigurator matchaConfigurator;
    private IModelConfigurator vitsConfigurator;
    private IOptions<TtsOptions> options;
    private ModelLoader modelLoader;
    private string tempDirectory;

    [SetUp]
    public void SetUp() {
        logger = Substitute.For<ILogger<ModelLoader>>();
        options = Options.Create(new TtsOptions());

        // Mock configurators
        kokoroConfigurator = Substitute.For<IModelConfigurator>();
        kokoroConfigurator.SupportedType.Returns(ModelType.Kokoro);

        matchaConfigurator = Substitute.For<IModelConfigurator>();
        matchaConfigurator.SupportedType.Returns(ModelType.Matcha);

        vitsConfigurator = Substitute.For<IModelConfigurator>();
        vitsConfigurator.SupportedType.Returns(ModelType.Vits);

        var configurators = new List<IModelConfigurator> {
            kokoroConfigurator,
            matchaConfigurator,
            vitsConfigurator
        };

        modelLoader = new ModelLoader(logger, options, configurators);

        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        tempDirectory = Path.Combine(Path.GetTempPath(), "SherpaOnnxTtsTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(tempDirectory)) {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Test]
    public void DetectModelType_KokoroFilesExist_ReturnsKokoro() {
        File.Create(Path.Combine(tempDirectory, "model.onnx")).Dispose();
        File.Create(Path.Combine(tempDirectory, "voices.bin")).Dispose();

        ModelType type = modelLoader.DetectModelType(tempDirectory);

        Assert.That(type, Is.EqualTo(ModelType.Kokoro));
    }

    [Test]
    public void DetectModelType_MatchaFilesExist_ReturnsMatcha() {
        File.Create(Path.Combine(tempDirectory, "model-steps-3.onnx")).Dispose();

        ModelType type = modelLoader.DetectModelType(tempDirectory);

        Assert.That(type, Is.EqualTo(ModelType.Matcha));
    }

    [Test]
    public void DetectModelType_VitsFilesExist_ReturnsVits() {
        File.Create(Path.Combine(tempDirectory, "vits-model.onnx")).Dispose();

        ModelType type = modelLoader.DetectModelType(tempDirectory);

        Assert.That(type, Is.EqualTo(ModelType.Vits));
    }

    [Test]
    public void DetectModelType_VitsFilesWithVocos_ReturnsUnknown() {
        File.Create(Path.Combine(tempDirectory, "vocos.onnx")).Dispose();

        ModelType type = modelLoader.DetectModelType(tempDirectory);

        Assert.That(type, Is.EqualTo(ModelType.Unknown));
    }

    [Test]
    public void DetectModelType_EmptyDirectory_ReturnsUnknown() {
        ModelType type = modelLoader.DetectModelType(tempDirectory);

        Assert.That(type, Is.EqualTo(ModelType.Unknown));
    }

    [Test]
    public void LoadModel_NoConfiguratorFound_ReturnsNullAndLogsError() {
        // Setup configuration to return a type we haven't mocked (or remove one from list)
        // But easier here: force a type return that logic detects but we don't provider config for?
        // Actually, our Detect logic is hardcoded. 
        // Let's just pass an empty list to a new loader instance for this test specifically.

        var emptyLoader = new ModelLoader(logger, options, new List<IModelConfigurator>());
        File.Create(Path.Combine(tempDirectory, "model.onnx")).Dispose();
        File.Create(Path.Combine(tempDirectory, "voices.bin")).Dispose();

        ModelInfo? result = emptyLoader.LoadModel(tempDirectory, "kokoro-model");

        Assert.That(result, Is.Null);

        // Check for specific error log about missing configurator
        bool errorLogged = logger.ReceivedCalls().Any(call =>
            call.GetMethodInfo().Name == "Log" &&
            (LogLevel?)call.GetArguments()[0] == LogLevel.Error);

        Assert.That(errorLogged, Is.True);
    }

    [Test]
    public void LoadModel_ConfiguratorThrowsException_ReturnsNullAndLogsError() {
        File.Create(Path.Combine(tempDirectory, "model.onnx")).Dispose();
        File.Create(Path.Combine(tempDirectory, "voices.bin")).Dispose();

        // Simulate configurator validation error
        // Note: For ref arguments, we use WhenForAnyArgs and pass dummy variables 
        // because Arg.Any<T>() cannot be passed as ref.
        var dummyConfig = new OfflineTtsConfig();
        kokoroConfigurator.WhenForAnyArgs(x => x.Configure(ref dummyConfig, string.Empty))
            .Do(x => throw new FileNotFoundException("Missing file"));

        ModelInfo? result = modelLoader.LoadModel(tempDirectory, "broken-kokoro");

        Assert.That(result, Is.Null);

        bool errorLogged = logger.ReceivedCalls().Any(call =>
            call.GetMethodInfo().Name == "Log" &&
            (LogLevel?)call.GetArguments()[0] == LogLevel.Error
        );
        Assert.That(errorLogged, Is.True);
    }
}
