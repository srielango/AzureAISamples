using AIChat.Configuration;
using AIChat.Models;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Options;

namespace AIChat.Services;

public class TextAnalysisService
{
    private AISettingsOption _settings;

    public TextAnalysisService(IOptions<AISettingsOption> options)
    {
        _settings = options.Value;
    }

    public async Task<TextAnalysisResult> AnalyzeAsync(string text)
    {
        var credentials = new AzureKeyCredential(_settings.TextAnalysis.ApiKey);
        var endPoint = new Uri(_settings.TextAnalysis.Endpoint);
        TextAnalyticsClient aiClient = new TextAnalyticsClient(endPoint, credentials);

        return await Analyze(aiClient, text) ;
    }

    private async Task<TextAnalysisResult> Analyze(TextAnalyticsClient aiClient, string text)
    {
        var documentDetails = new TextAnalysisResult();

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
