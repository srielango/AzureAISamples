using AIChat.Configuration;
using AIChat.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

public class SpeechService
{
    private readonly AISettingsOption _settings;
    private readonly string _key;
    private readonly string _region;
    private readonly string _translatorKey;
    private readonly IWebHostEnvironment _env;

    private readonly Dictionary<string, string> _voices = new()
    {
        ["ta"] = "ta-IN-PallaviNeural",
        ["hi"] = "hi-IN-MadhurNeural",
        ["fr"] = "fr-FR-DeniseNeural",
        ["de"] = "de-DE-KatjaNeural",
        ["en"] = "en-AU-NatashaNeural",
        ["ar"] = "ar-AE-FatimaNeural"
    };

    public SpeechService(IOptions<AISettingsOption> options, IWebHostEnvironment env)
    {
        _settings = options.Value;
        _key = _settings.SpeechKey;
        _region = _settings.SpeechRegion;
        _translatorKey = _settings.TranslatorKey;
        _env = env;
    }

    public async Task<SpeechTranslatorResponse> ProcessAudioAsync(string base64Audio, string toLanguage)
    {
        var audioBytes = Convert.FromBase64String(base64Audio);
        using var audioContent = new ByteArrayContent(audioBytes);
        string tempFile = Path.Combine(_env.ContentRootPath, "TempAudio", $"{Guid.NewGuid()}.wav");
        Directory.CreateDirectory(Path.GetDirectoryName(tempFile)!);
        await File.WriteAllBytesAsync(tempFile, audioBytes);

        string normalizedPath = NormalizeWavTo16kHzMono(tempFile);
        var recognizedText = await RecognizeSpeechAsync(normalizedPath);
        var response = await TranslateTextAsync(recognizedText, toLanguage);

        File.Delete(normalizedPath);

        File.Delete(tempFile); // Clean up
        return response;
    }
    public async Task<SpeechTranslatorResponse> TranslateTextAsync(string text, string targetLang)
    {
        var endpoint = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={targetLang}";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _translatorKey);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _region);

        var requestBody = new[]
        {
            new { Text = text }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);
        var translatedText = doc.RootElement[0]
            .GetProperty("translations")[0]
            .GetProperty("text")
            .GetString();

        var audioData = await SynthesizeAsync(translatedText, targetLang);
        return new SpeechTranslatorResponse(text, translatedText, audioData);
    }
    private async Task<byte[]> SynthesizeAsync(string text, string lang)
    {
        var config = SpeechConfig.FromSubscription(_key, _region);
        config.SpeechSynthesisVoiceName = _voices[lang];
        using var synthesizer = new SpeechSynthesizer(config, null);
        var result = await synthesizer.SpeakTextAsync(text);
        return result.AudioData;
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
}
