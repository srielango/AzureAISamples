using AIChat;
using AIChat.Components;
using AIChat.Configuration;
using AIChat.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<AISettingsOption>(builder.Configuration.GetSection(AISettingsOption.Name));
builder.Services.AddScoped<ITextAnalysisService, TextAnalysisService>();

builder.Services.AddScoped<AzureOpenAIClientHelper>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
