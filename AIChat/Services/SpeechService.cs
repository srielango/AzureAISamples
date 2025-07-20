using AIChat.Configuration;
using AIChat.Models;
using AIChat.Services;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Options;

public class SpeechService : ISpeechService
{
    private readonly AISettingsOption _settings;
    private readonly string _key;
    private readonly string _region;
    private readonly Dictionary<string, string> _voices = new()
    {
        ["ta"] = "ta-IN-PallaviNeural",
        ["hi"] = "hi-IN-MadhurNeural"
    };

    public SpeechService(IOptions<AISettingsOption> options)
    {
        _settings = options.Value;
        _key = _settings.SpeechKey;
        _region = _settings.SpeechRegion;
    }

    public async Task<SpeechTranslatorResponse> TranslateAndSpeakAsync(string targetLang)
    {
        var translationConfig = CreateTranslationConfig(targetLang);
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new TranslationRecognizer(translationConfig, audioConfig);

        var result = await recognizer.RecognizeOnceAsync();
        if (result.Reason != ResultReason.TranslatedSpeech)
            throw new Exception($"Speech recognition failed: {result.Reason}");

        var recognizedText = result.Text;
        var translatedText = result.Translations[targetLang];
        var audioData = await SynthesizeAsync(translatedText, targetLang);
        return new SpeechTranslatorResponse(recognizedText, translatedText, audioData);
    }

    public async Task StartContinuousTranslationAsync(
        string targetLang,
        Action<string, string> onTranslationReceived,
        Action onCompleted,
        Action<string> onError)
    {
        var translationConfig = CreateTranslationConfig(targetLang);
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new TranslationRecognizer(translationConfig, audioConfig);

        recognizer.Recognized += (s, e) => HandleRecognized(e, targetLang, onTranslationReceived);
        recognizer.Canceled += (s, e) => onError?.Invoke(e.ErrorDetails);
        recognizer.SessionStopped += (s, e) => onCompleted?.Invoke();

        try
        {
            await recognizer.StartContinuousRecognitionAsync();
            await Task.Delay(10000); // Listen for 10 seconds
            await recognizer.StopContinuousRecognitionAsync();
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    private void HandleRecognized(TranslationRecognitionEventArgs e, string targetLang, Action<string, string> onTranslationReceived)
    {
        if (e.Result.Reason == ResultReason.TranslatedSpeech)
        {
            var recognized = e.Result.Text;
            var translated = e.Result.Translations[targetLang];
            onTranslationReceived?.Invoke(recognized, translated);
        }
    }

    private SpeechTranslationConfig CreateTranslationConfig(string targetLang)
    {
        var config = SpeechTranslationConfig.FromSubscription(_key, _region);
        config.SpeechRecognitionLanguage = "en-US";
        config.AddTargetLanguage(targetLang);
        return config;
    }

    public async Task<byte[]> SynthesizeAsync(string text, string lang)
    {
        var config = SpeechConfig.FromSubscription(_key, _region);
        config.SpeechSynthesisVoiceName = _voices[lang];
        using var synthesizer = new SpeechSynthesizer(config, null);
        var result = await synthesizer.SpeakTextAsync(text);
        return result.AudioData;
    }
}
