using Azure.AI.TextAnalytics;

namespace AIChat.Models;

public class TextAnalysisResult
{
    public string OriginalText { get; set; } = string.Empty;
    public string DetectedLanguage { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public List<string> KeyPhrases { get; set; } = new();
    public List<CategorizedEntity> Entities { get; set; } = new();
    public List<LinkedEntity> LinkedEntities { get; set; } = new();
}
