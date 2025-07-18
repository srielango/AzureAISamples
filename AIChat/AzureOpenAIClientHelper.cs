using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using OpenAI.Chat;

namespace AIChat;

public class AzureOpenAIClientHelper
{
    private readonly IConfiguration _config;

    public AzureOpenAIClientHelper(IConfiguration config)
    {
        _config = config;
    }

    public string GetChatResponse(List<ChatMessage> messages)
    {
        string endpoint = _config["AzureOpenAI:Endpoint"] ?? string.Empty;
        string apiKey = _config["AzureOpenAI:ApiKey"] ?? string.Empty;
        string chatModel = _config["AzureOpenAI:ChatModel"] ?? string.Empty;
        string embeddingModel = _config["AzureOpenAI:EmbeddingModel"] ?? string.Empty;
        string searchEndpoint = _config["AzureSearch:Endpoint"] ?? string.Empty;
        string searchKey = _config["AzureSearch:Key"] ?? string.Empty;
        string searchIndex = _config["AzureSearch:Index"] ?? string.Empty;

        AzureOpenAIClient client = new(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey));
        
        ChatClient chatClient = client.GetChatClient(chatModel);

        ChatCompletionOptions options = new();

#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        options.AddDataSource(new AzureSearchChatDataSource()
        {
            Endpoint = new Uri(searchEndpoint),
            IndexName = searchIndex,
            Authentication = DataSourceAuthentication.FromApiKey(searchKey),
            QueryType = "vector",
            VectorizationSource = DataSourceVectorizer.FromDeploymentName(embeddingModel)
        });
#pragma warning restore AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        ChatCompletion completion = chatClient.CompleteChat(messages, options);
        var completionText = completion.Content[0].Text;

        return completionText ?? "No response.";
    }
}
