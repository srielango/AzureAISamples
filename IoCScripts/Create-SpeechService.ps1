# Variables
$resourceGroup = "elango-ai-rg"
$location = "eastus"
$speechServiceName = "elango-ai-speech-service"
$sku = "F0"

# Step 1: Create Resource Group (if not exists)
az group create --name $resourceGroup --location $location

# Step 2: Create Speech Service
az cognitiveservices account create `
  --name $speechServiceName `
  --resource-group $resourceGroup `
  --kind SpeechServices `
  --sku $sku `
  --location $location `
  --yes

# Step 3: Confirm
Write-Output "Azure AI Speech service '$speechServiceName' created in resource group '$resourceGroup'."
