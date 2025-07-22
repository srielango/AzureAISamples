using AIChat.Configuration;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class NewSpeechService
{
    private readonly HttpClient _httpClient;
    private readonly AISettingsOption _settings;
    private readonly string _key;
    private readonly string _region;
    private readonly string _translatorKey;

    private readonly IWebHostEnvironment _env;

    public NewSpeechService(HttpClient httpClient, IOptions<AISettingsOption> options, IWebHostEnvironment env)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _key = _settings.SpeechKey;
        _region = _settings.SpeechRegion;
        _translatorKey = _settings.TranslatorKey;
        _env = env;
    }

    public async Task<(string RecognizedText, string TranslatedText)> ProcessAudioAsync(string base64Audio, string fromLanguage, string toLanguage)
    {
        var audioBytes = Convert.FromBase64String(base64Audio);
        using var audioContent = new ByteArrayContent(audioBytes);
        string tempFile = Path.Combine(_env.ContentRootPath, "TempAudio", $"{Guid.NewGuid()}.wav");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        await File.WriteAllBytesAsync(tempFile, audioBytes);

        string normalizedPath = NormalizeWavTo16kHzMono(tempFile);
        var recognizedText = await RecognizeSpeechAsync(normalizedPath);
//        var recognizedText = await RecognizeSpeechAsync(tempFile);
        var translatedText = await TranslateTextAsync(recognizedText);

        File.Delete(normalizedPath);

        File.Delete(tempFile); // Clean up
        return (recognizedText, translatedText);
    }

    private string NormalizeWavTo16kHzMono(string inputPath)
    {
        var outputPath = Path.Combine(Path.GetDirectoryName(inputPath)!, $"{Guid.NewGuid()}-normalized.wav");

        using var reader = new NAudio.Wave.AudioFileReader(inputPath);
        var resampler = new NAudio.Wave.MediaFoundationResampler(reader, new NAudio.Wave.WaveFormat(16000, 16, 1));
        NAudio.Wave.WaveFileWriter.CreateWaveFile(outputPath, resampler);

        return outputPath;
    }


    private async Task<string> RecognizeSpeechAsync(string wavFile)
    {
        var config = SpeechConfig.FromSubscription(_key, _region);

        using var audioInput = AudioConfig.FromWavFileInput(wavFile);
        using var recognizer = new SpeechRecognizer(config, audioInput);

        var result = await recognizer.RecognizeOnceAsync();
        return result.Reason == ResultReason.RecognizedSpeech ? result.Text : "";
    }

    private async Task<string> TranslateTextAsync(string inputText)
    {
        var endpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=ta"; // target = Tamil

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _translatorKey);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _region);

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
