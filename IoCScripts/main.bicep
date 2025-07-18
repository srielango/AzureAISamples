param location string = 'eastus'
param openAIName string = 'ai-elangoopenaidemo'
param searchServiceName string = 'elango-openai-search-demo'
param storageAccountName string = 'elangobrochurestore'

resource openAI 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIName
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource search 'Microsoft.Search/searchServices@2020-08-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: 'standard'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}
