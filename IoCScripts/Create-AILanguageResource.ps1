# Create-AILanguageResource.ps1
param (
    [string]$ResourceGroup = "elango-ai-rg",
    [string]$Location = "eastus",
    [string]$ResourceName = "elango-language-service",
    [string]$Sku = "F0"  # F0 = Free tier
)

Write-Host "Creating resource group if not exists..."
az group create --name $ResourceGroup --location $Location

Write-Host "Creating Azure Language resource..."
az cognitiveservices account create `
    --name $ResourceName `
    --resource-group $ResourceGroup `
    --kind TextAnalytics `
    --sku $Sku `
    --location $Location `
    --yes `
    --custom-domain $ResourceName `
    --tags Environment=Dev Project=AzureAI `
    --output table

Write-Host "Azure AI Language service created successfully!"