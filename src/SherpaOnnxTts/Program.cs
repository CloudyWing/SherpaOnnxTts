using System.Text.Json.Serialization;
using CloudyWing.SherpaOnnxTts.Endpoints;
using CloudyWing.SherpaOnnxTts.Extensions;
using CloudyWing.SherpaOnnxTts.Services;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddOpenApi();
builder.Services.AddTtsServices(builder.Configuration);

WebApplication app = builder.Build();

ITtsService ttsService = app.Services.GetRequiredService<ITtsService>();
await ttsService.InitializeAsync();

app.MapOpenApi();
app.MapScalarApiReference(options => {
    options.WithTitle("SherpaOnnx TTS API");
    options.WithTheme(ScalarTheme.DeepSpace);
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseStaticFiles();

app.MapTtsEndpoints();

app.Run();
