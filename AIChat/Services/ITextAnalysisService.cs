using AIChat.Models;

namespace AIChat.Services;

public interface ITextAnalysisService
{
    Task<DocumentAnalysisResult> AnalyzeAsync(string text);
}
