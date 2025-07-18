az group create --name rag-demo-rg --location eastus
az deployment group create --resource-group rag-demo-rg --template-file main.bicep
