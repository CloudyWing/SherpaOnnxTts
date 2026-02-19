using CloudyWing.SherpaOnnxTts.Models;
using CloudyWing.SherpaOnnxTts.Models.Api;
using CloudyWing.SherpaOnnxTts.Services;
using CloudyWing.SherpaOnnxTts.Utils;

namespace CloudyWing.SherpaOnnxTts.Endpoints;

/// <summary>
/// Extension methods for mapping TTS API endpoints.
/// </summary>
public static class TtsEndpoints {
    private const string HealthCheckEndpoint = "/health";
    private const string TtsGetEndpoint = "/tts";
    private const string OpenAiSpeechEndpoint = "/v1/audio/speech";

    /// <summary>
    /// Maps the TTS API endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapTtsEndpoints(this IEndpointRouteBuilder endpoints) {
        endpoints.MapHealthCheck();
        endpoints.MapTtsGet();
        endpoints.MapOpenAiSpeech();

        return endpoints;
    }

    private static void MapHealthCheck(this IEndpointRouteBuilder endpoints) {
        endpoints.MapGet(HealthCheckEndpoint, (ITtsService service) => {
            HealthCheckResponse response = new(
                Status: "ok",
                LoadedModels: service.GetLoadedModels().Select(m => new ModelHealthInfo(
                    Name: m.Name,
                    Type: m.Type.ToString(),
                    SampleRate: m.SampleRate,
                    NumSpeakers: m.NumSpeakers
                )).ToList(),
                DefaultVoice: service.GetDefaultVoice()
            );

            return Results.Ok(response);
        })
        .WithName("HealthCheck")
        .WithSummary("Check service health and loaded models")
        .WithDescription("Returns the service status, list of loaded models with details, and the default voice")
        .Produces<HealthCheckResponse>();
    }

    private static void MapTtsGet(this IEndpointRouteBuilder endpoints) {
        endpoints.MapGet(
            TtsGetEndpoint,
            (string text, string? voice, int? sid, float? speed, string? format, ITtsService service) => {
                if (string.IsNullOrWhiteSpace(text)) {
                    return Results.BadRequest("'text' parameter is required and cannot be empty");
                }

                // Default to WAV for download
                string outputFormat = string.IsNullOrWhiteSpace(format) ? AudioFormatUtils.Wav : format;

                TtsGenerationParameters parameters = new() {
                    Text = text,
                    Voice = voice,
                    SpeakerId = sid ?? 0,
                    Speed = speed ?? 1.0F,
                    Format = outputFormat
                };

                GeneratedAudio? audio = service.GenerateAudio(parameters);
                if (audio is null) {
                    return Results.Problem("Failed to generate audio", statusCode: 500);
                }

                if (outputFormat.Equals(AudioFormatUtils.Pcm, StringComparison.OrdinalIgnoreCase)) {
                    byte[] pcmBytes = AudioFormatUtils.ToPcmBytes(audio.Samples);
                    return Results.File(pcmBytes, AudioFormatUtils.PcmContentType, "speech.pcm");
                }

                // Default to WAV
                byte[] wavBytes = WaveUtils.Encode(audio.SampleRate, audio.Samples);
                return Results.File(wavBytes, AudioFormatUtils.WavContentType, "speech.wav");
            }
        )
        .WithName("GenerateTtsGet")
        .WithSummary("Generate speech from text (GET)")
        .WithDescription("Generates speech audio file (WAV/PCM) for download")
        .Produces(StatusCodes.Status200OK, contentType: AudioFormatUtils.WavContentType)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    private static void MapOpenAiSpeech(this IEndpointRouteBuilder endpoints) {
        endpoints.MapPost(
            OpenAiSpeechEndpoint,
            async (OpenAiSpeechRequest request, ITtsService service, HttpResponse response) => {
                if (string.IsNullOrWhiteSpace(request.Input)) {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    await response.WriteAsJsonAsync("input must be a non-empty string");
                    return;
                }

                // Strictly enforce PCM Streaming for OpenAI Endpoint
                // We ignore 'format' requested by client if it's wav/mp3 because we can't stream them efficiently/correctly yet.
                // We always stream PCM.

                // Mapping OpenAI -> SherpaOnnx
                // model -> Voice (The model folder name)
                // voice -> SpeakerId (The speaker ID, parsed from string. Defaults to 0)

                string? modelName = request.Model;
                int speakerId = 0;

                // 1. Try parsing as direct Integer ID (e.g., "0", "1")
                if (!string.IsNullOrWhiteSpace(request.Voice) && int.TryParse(request.Voice, out int parsedSid)) {
                    speakerId = parsedSid;
                }
                // 2. Try looking up by Name (e.g., "af_bella" -> 2)
                else if (!string.IsNullOrWhiteSpace(request.Voice) && KokoroVoiceMap.TryGetValue(request.Voice.ToLowerInvariant(), out int mappedId)) {
                    speakerId = mappedId;
                }

                TtsGenerationParameters parameters = new() {
                    Text = request.Input,
                    Voice = modelName, // internal 'Voice' param is the model name
                    Speed = request.Speed ?? 1.0F,
                    SpeakerId = speakerId,
                    Format = AudioFormatUtils.Pcm
                };

                response.StatusCode = StatusCodes.Status200OK;
                response.ContentType = AudioFormatUtils.PcmContentType;

                // Inform client about the sample rate so they can play it back correctly
                int sampleRate = service.GetModelSampleRate(parameters.Voice);
                response.Headers.Append("X-Sherpa-Sample-Rate", sampleRate.ToString());

                // No Content-Disposition, this is a stream.

                await foreach (byte[] chunk in service.GenerateAudioStream(parameters)) {
                    await response.Body.WriteAsync(chunk);
                    await response.Body.FlushAsync();
                }
            }
        )
        .WithName("OpenAiSpeech")
        .WithSummary("Generate speech (OpenAI-compatible Stream)")
        .WithDescription("Streams raw PCM audio (f32le). Always streaming, no file download.")
        .Produces(StatusCodes.Status200OK, contentType: AudioFormatUtils.PcmContentType)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }

    // Mapping for Kokoro v1.0 (50+ speakers). 
    // This allows users to use "af_bella" directly instead of remembering ID "2".
    private static readonly Dictionary<string, int> KokoroVoiceMap = new() {
        { "af_alloy", 0 },
        { "af_aoede", 1 },
        { "af_bella", 2 },
        { "af_jessica", 3 },
        { "af_kore", 4 },
        { "af_nicole", 11 },
        { "af_sky", 21 },
        { "am_adam", 30 },
        { "am_michael", 33 },
        // Fallback/Aliases for OpenAI compatibility
        { "alloy", 0 },    // Map OpenAI 'alloy' to af_alloy
        { "echo", 30 },    // Map OpenAI 'echo' to am_adam (Male)
        { "fable", 33 },   // Map OpenAI 'fable' to am_michael (Male)
        { "onyx", 33 },    // Map OpenAI 'onyx' to am_michael
        { "nova", 1 },     // Map OpenAI 'nova' to af_aoede (Female)
        { "shimmer", 2 },  // Map OpenAI 'shimmer' to af_bella (Female)
    };
}
