namespace CloudyWing.SherpaOnnxTts.Models.Api;

/// <summary>
/// Response model for health check endpoint.
/// </summary>
/// <param name="Status">The service status (e.g., "ok").</param>
/// <param name="LoadedModels">List of loaded TTS models.</param>
/// <param name="DefaultVoice">The name of the default voice.</param>
public record HealthCheckResponse(
    string Status,
    IReadOnlyList<ModelHealthInfo> LoadedModels,
    string DefaultVoice
);

public record ModelHealthInfo(
    string Name,
    string Type,
    int SampleRate,
    int NumSpeakers
);
