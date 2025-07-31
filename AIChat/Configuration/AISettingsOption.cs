namespace AIChat.Configuration;

//public class AISettingsOption
//{
//    public const string Name = "AISettings";
//    public string ServiceEndPoint { get; set; } = string.Empty;
//    public string ServiceKey { get; set; } = string.Empty;
//    public string SpeechKey { get; set; } = string.Empty;
//    public string SpeechRegion { get; set; } = string.Empty;
//    public string TranslatorKey { get; set; } = string.Empty;
//    public string OaiEndPoint { get; set; } = string.Empty;
//    public string OaiKey { get; set; } = string.Empty;
//    public string OaiDeployment { get; set; } = string.Empty;
//    public string AzureSearchEndpoint { get; set; } = string.Empty;
//    public string AzureSearchKey { get; set; } = string.Empty;
//    public string AzureSearchIndex { get; set; } = string.Empty;
//}

public class AISettingsOption
{
    public const string Name = "Azure";
    public TextAnalysisSettings TextAnalysis { get; set; } = new();
    public OpenAISettings OpenAI { get; set; } = new();
    public SearchSettings Search { get; set; } = new();
    public SpeechSettings Speech { get; set; } = new();
    public TranslatorSettings Translator { get; set; } = new();
}

public class TextAnalysisSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
public class OpenAISettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChatModel { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
}

public class SearchSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
}

public class SpeechSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Region { get; set;} = string.Empty;
}

public class TranslatorSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}
