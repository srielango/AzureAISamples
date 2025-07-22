using AIChat.Configuration;
using Azure;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AIChat.Services;

public class NewSpeechService
{
    private readonly AISettingsOption _settings;
    private readonly string _key;
    private readonly string _region;
    private readonly string _translatorKey;

    private readonly IWebHostEnvironment _env;

    private readonly Dictionary<string, string> _voices = new()
    {
        ["ta"] = "ta-IN-PallaviNeural",
        ["hi"] = "hi-IN-MadhurNeural"
    };

    public NewSpeechService(IOptions<AISettingsOption> options, IWebHostEnvironment env)
    {
        _settings = options.Value;
        _key = _settings.SpeechKey;
        _translatorKey = _settings.TranslatorKey;

        _region = _settings.SpeechRegion;

        _env = env;
    }

    public async Task<string> TranslateSpeechAsync(string base64Wav)
    {
        var audioBytes = Convert.FromBase64String(base64Wav);

        string tempFile = Path.Combine(_env.ContentRootPath, "TempAudio", $"{Guid.NewGuid()}.wav");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        await File.WriteAllBytesAsync(tempFile, audioBytes);

        var recognizedText = await RecognizeSpeechAsync(tempFile);
        var translatedText = await TranslateTextAsync(recognizedText);

        File.Delete(tempFile); // Clean up
        return translatedText;
    }

    private async Task<string> RecognizeSpeechAsync(string wavFile)
    {
        var speechKey = _key;
        var region = _region;
        var config = SpeechConfig.FromSubscription(speechKey, region);

        using var audioInput = AudioConfig.FromWavFileInput(wavFile);
        using var recognizer = new SpeechRecognizer(config, audioInput);

        var result = await recognizer.RecognizeOnceAsync();
        return result.Reason == ResultReason.RecognizedSpeech ? result.Text : "";
    }

    private async Task<string> TranslateTextAsync(string inputText)
    {
        var translatorKey = _translatorKey;
        var translatorRegion = _region;

        var endpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=ta"; // target = Tamil

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", translatorKey);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", translatorRegion);

        var requestBody = new[]
        {
        new { Text = inputText }
    };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);
        var translatedText = doc.RootElement[0]
            .GetProperty("translations")[0]
            .GetProperty("text")
            .GetString();

        return translatedText ?? string.Empty;
    }
}