using AIChat.Models;

namespace AIChat.Services
{
    public interface ISpeechService
    {
        Task StartContinuousTranslationAsync(string targetLang, Action<string, string> onTranslationReceived, Action onCompleted, Action<string> onError);
        Task<byte[]> SynthesizeAsync(string text, string lang);
        Task<SpeechTranslatorResponse> TranslateAndSpeakAsync(string targetLang);
        Task<SpeechTranslatorResponse> TranslateTextAsync(string text, string targetLang); // NEW: Translate typed text
    }
}
