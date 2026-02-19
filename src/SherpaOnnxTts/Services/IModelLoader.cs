using CloudyWing.SherpaOnnxTts.Models;

namespace CloudyWing.SherpaOnnxTts.Services;

/// <summary>
/// Interface for loading TTS models.
/// </summary>
public interface IModelLoader {
    /// <summary>
    /// Loads a TTS model from a directory.
    /// </summary>
    /// <param name="modelDirectory">Directory containing the model files</param>
    /// <param name="modelName">Name of the model</param>
    /// <returns>Model information if successful, null otherwise</returns>
    ModelInfo? LoadModel(string modelDirectory, string modelName);

    /// <summary>
    /// Detects the type of model in a directory.
    /// </summary>
    ModelType DetectModelType(string modelDirectory);
}
