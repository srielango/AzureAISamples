namespace AIChat.Configuration;

public class AISettingsOption
{
    public const string Name = "AISettings";
    public string ServiceEndPoint { get; set; } = string.Empty;
    public string ServiceKey { get; set; } = string.Empty;
    public string SpeechKey { get; set; } = string.Empty;
    public string SpeechRegion { get; set; } = string.Empty;
    public string TranslatorKey { get; set; } = string.Empty;
    public string OaiEndPoint { get; set; } = string.Empty;
    public string OaiKey { get; set; } = string.Empty;
    public string OaiDeployment { get; set; } = string.Empty;
    public string AzureSearchEndpoint { get; set; } = string.Empty;
    public string AzureSearchKey { get; set; } = string.Empty;
    public string AzureSearchIndex { get; set; } = string.Empty;
}
