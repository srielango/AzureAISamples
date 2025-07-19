using AIChat.Configuration;
using AIChat.Models;
using AIChat.Services;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Options;

public class SpeechService : ISpeechService
{
    private AISettingsOption _settings;
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
        var translationConfig = SpeechTranslationConfig.FromSubscription(_key, _region);
        translationConfig.SpeechRecognitionLanguage = "en-US";
        translationConfig.AddTargetLanguage(targetLang);

        string recognizedText = "", translatedText = "";

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new TranslationRecognizer(translationConfig, audioConfig);

        var result = await recognizer.RecognizeOnceAsync();
        if (result.Reason == ResultReason.TranslatedSpeech)
        {
            recognizedText = result.Text;
            translatedText = result.Translations[targetLang];
        }
        else
        {
            throw new Exception("Speech recognition failed: " + result.Reason.ToString());
        }

        var speechConfig = SpeechConfig.FromSubscription(_key, _region);
        speechConfig.SpeechSynthesisVoiceName = _voices[targetLang];

        using var synthesizer = new SpeechSynthesizer(speechConfig, null);
        var speechResult = await synthesizer.SpeakTextAsync(translatedText);

        return new SpeechTranslatorResponse (recognizedText, translatedText, speechResult.AudioData);
    }

    public async Task StartContinuousTranslationAsync(
        string targetLang,
        Action<string, string> onTranslationReceived,
        Action onCompleted,
        Action<string> onError)
    {
        try
        {
            var translationConfig = SpeechTranslationConfig.FromSubscription(_key, _region);
            translationConfig.SpeechRecognitionLanguage = "en-US";
            translationConfig.AddTargetLanguage(targetLang);

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new TranslationRecognizer(translationConfig, audioConfig);

            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.TranslatedSpeech)
                {
                    var recognized = e.Result.Text;
                    var translated = e.Result.Translations[targetLang];
                    onTranslationReceived?.Invoke(recognized, translated);
                }
            };

            recognizer.Canceled += (s, e) => onError?.Invoke(e.ErrorDetails);
            recognizer.SessionStopped += (s, e) => onCompleted?.Invoke();

            await recognizer.StartContinuousRecognitionAsync();
            await Task.Delay(10000); // Listen for 10 seconds
            await recognizer.StopContinuousRecognitionAsync();
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
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
