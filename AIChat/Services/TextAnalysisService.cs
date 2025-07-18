using AIChat.Configuration;
using AIChat.Models;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Options;

namespace AIChat.Services;

public class TextAnalysisService : ITextAnalysisService
{
    private AISettingsOption _settings;

    public TextAnalysisService(IOptions<AISettingsOption> options)
    {
        _settings = options.Value;
    }

    public async Task<DocumentAnalysisResult> AnalyzeAsync(string text)
    {
        var credentials = new AzureKeyCredential(_settings.ServiceKey);
        var endPoint = new Uri(_settings.ServiceEndPoint);
        TextAnalyticsClient aiClient = new TextAnalyticsClient(endPoint, credentials);

        return await Analyze(aiClient, text) ;
    }

    private async Task<DocumentAnalysisResult> Analyze(TextAnalyticsClient aiClient, string text)
    {
        var documentDetails = new DocumentAnalysisResult();

        documentDetails.OriginalText = text;

        documentDetails.DetectedLanguage = await DetectLanguage(aiClient, text);

        documentDetails.Sentiment = await GetSentiment(aiClient, text);

        documentDetails.KeyPhrases = await ExtractKeyPhrases(aiClient, text);

        documentDetails.Entities = await GetEntities(aiClient, text);

        documentDetails.LinkedEntities = await GetLinkedEntities(aiClient, text);

        return documentDetails;
    }

    private async Task<List<LinkedEntity>> GetLinkedEntities(TextAnalyticsClient aiClient, string text)
    {
        List<LinkedEntity> entities = new List<LinkedEntity>();
        LinkedEntityCollection linkedEntities = await aiClient.RecognizeLinkedEntitiesAsync(text);
        if (linkedEntities.Any())
        {
            entities.AddRange(linkedEntities);
        }
        return entities;
    }


    private async Task<List<CategorizedEntity>> GetEntities(TextAnalyticsClient aiClient, string text)
    {
        List<CategorizedEntity> entites = new();

        CategorizedEntityCollection extractedEntities = await aiClient.RecognizeEntitiesAsync(text);
        if (extractedEntities.Any())
        {
            entites.AddRange(extractedEntities);
        }
        return entites;
    }

    private async Task<List<string>> ExtractKeyPhrases(TextAnalyticsClient aiClient, string text)
    {
        List<string> ExtractKeyPhrases = new List<string>();

        KeyPhraseCollection phrases = await aiClient.ExtractKeyPhrasesAsync(text);
        if (phrases.Count > 0)
        {
            ExtractKeyPhrases.AddRange(phrases);
        }

        return ExtractKeyPhrases;
    }

    private async Task<string> GetSentiment(TextAnalyticsClient aiClient, string text)
    {
        DocumentSentiment sentiment = await aiClient.AnalyzeSentimentAsync(text);
        return sentiment.Sentiment.ToString();
    }

    private async Task<string> DetectLanguage(TextAnalyticsClient aiClient, string text)
    {
        DetectedLanguage detectedLanguage = await aiClient.DetectLanguageAsync(text);
        return detectedLanguage.Name;
    }
}
