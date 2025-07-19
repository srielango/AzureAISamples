namespace AIChat.Models;

public record SpeechTranslatorResponse (string RecognizedText, string TranslatedText, byte[] Audio );
