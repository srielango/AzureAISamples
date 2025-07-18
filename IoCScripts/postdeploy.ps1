# Variables
$resourceGroup = "rag-demo-rg"
$storageAccount = "elangobrochurestore"
$searchService = "elango-openai-search-demo"
$containerName = "brochures"
$localFolder = "./brochures"
$indexName = "brochures-index"

# Get keys
$storageKey = az storage account keys list `
  --account-name $storageAccount `
  --resource-group $resourceGroup `
  --query "[0].value" -o tsv

$searchAdminKey = az search admin-key show `
  --service-name $searchService `
  --resource-group $resourceGroup `
  --query "primaryKey" -o tsv

$searchEndpoint = "https://$searchService.search.windows.net"

# Upload PDFs to blob container
Get-ChildItem -Path $localFolder -Filter *.pdf | ForEach-Object {
    Write-Host "Uploading $($_.Name)..."
    az storage blob upload `
        --account-name $storageAccount `
        --account-key $storageKey `
        --container-name $containerName `
        --name $_.Name `
        --file $_.FullName `
        --overwrite
}

# Create search index
$indexDefinition = @{
  name = $indexName
  fields = @(
    @{ name = "id"; type = "Edm.String"; key = $true; filterable = $true },
    @{ name = "content"; type = "Edm.String"; searchable = $true },
    @{ name = "metadata_storage_name"; type = "Edm.String"; searchable = $true }
  )
} | ConvertTo-Json -Depth 5


Invoke-RestMethod -Method PUT -Uri "$searchEndpoint/indexes/$indexName?api-version=2023-11-01" -Headers @{ "api-key" = $searchAdminKey; "Content-Type" = "application/json" } -Body $indexDefinition

# Create data source
$datasource = @"
{
  "name": "brochures-ds",
  "type": "azureblob",
  "credentials": {
    "connectionString": "DefaultEndpointsProtocol=https;AccountName=$storageAccount;AccountKey=$storageKey"
  },
  "container": {
    "name": "$containerName"
  }
}
"@

Invoke-RestMethod -Method PUT `
  -Uri "$searchEndpoint/datasources/brochures-ds?api-version=2023-11-01" -Headers @{ "api-key" = $searchAdminKey; "Content-Type" = "application/json" } -Body $datasource

# Create indexer
$indexer = @"
{
  "name": "brochures-indexer",
  "dataSourceName": "brochures-ds",
  "targetIndexName": "$indexName",
  "schedule": {
    "interval": "PT30M"
  },
  "parameters": {
    "configuration": {
      "parsingMode": "default",
      "indexedFileNameExtensions": ".pdf",
      "failOnUnsupportedContentType": false,
      "failOnError": false
    }
  }
}
"@

Invoke-RestMethod -Method PUT -Uri "$searchEndpoint/indexers/brochures-indexer?api-version=2023-11-01" -Headers @{ "api-key" = $searchAdminKey; "Content-Type" = "application/json" } -Body $indexer
