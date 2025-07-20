# AI-Powered Blazor Chat App with Text Analysis and Speech Translation

Welcome to my AI-powered Blazor Server application, showcasing advanced integration of **Azure Language Service** and **Azure Speech Service** with **.NET 8**. This project demonstrates a Retrieval-Augmented Generation (RAG) chatbot using OpenAI, text analysis capabilities, and real-time speech translation from English to Tamil or Hindi.

📧 **Contact**: [srielango@gmail.com](mailto:srielango@gmail.com) | [LinkedIn Profile](https://www.linkedin.com/in/elango-srinivasan/) | [Upwork Profile](https://www.linkedin.com/in/elango-srinivasan/)

## Features

- **RAG Chatbot**: Interactive chat powered by OpenAI with document retrieval for context-aware responses.
- **Text Analysis**: Analyze user-provided text or uploaded PDF files for:
  - **Detected Language**: Identifies the language of the input text.
  - **Sentiment**: Determines positive, negative, or neutral sentiment.
  - **Key Phrases**: Extracts key phrases for summarization.
  - **Entities**: Identifies entities like people, places, or organizations.
  - **Linked Entities**: Provides context-aware entity linking to external knowledge bases.
- **Speech Translation**: Real-time speech-to-text and translation from English to Tamil or Hindi using Azure Speech Service (currently under maintenance).
- **Blazor Server**: Built with .NET 8 for a responsive, server-side web experience.
- **Azure Integration**: Leverages Azure services for scalable AI-powered features.

## Tech Stack

- **Frontend**: Blazor Server (.NET 8)
- **Backend Services**: Azure Language Service, Azure Speech Service, OpenAI API
- **Other**: SignalR for real-time updates, PDF parsing for text extraction

## Setup Instructions

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/srielango/AzureAISamples.git
   cd AzureAISamples
   ```

2. **Install Dependencies**:
   Ensure you have the **.NET 8 SDK** installed. Run:
   ```bash
   dotnet restore
   ```

3. **Configure Azure Services**:
   - Set up **Azure Language Service** and **Azure Speech Service** in the Azure Portal.
   - Add API keys to `appsettings.json` or use .NET Secret Manager:
     ```bash
     dotnet user-secrets set "Azure:Language:Key" "your-language-key"
     dotnet user-secrets set "Azure:Speech:Key" "your-speech-key"
     dotnet user-secrets set "OpenAI:ApiKey" "your-openai-key"
     ```

4. **Run the App**:
   ```bash
   dotnet run
   ```

## Screenshots

<img width="677" height="787" alt="image" src="https://github.com/user-attachments/assets/bc11c4cc-00e7-486b-833f-dc1bdc9d194f" />

<img width="576" height="572" alt="image" src="https://github.com/user-attachments/assets/76cefb42-4115-4446-9500-2a77162cbecb" />


## About Me

I’m a **Microsoft Certified Developer Associate**, **DevOps Expert**, and **AI Engineer Associate** with over 23 years of experience in .NET, Azure, and microservices. Based in Chennai, India, I specialize in microservices architecture, scalable and AI-powered web applications. Contact me to discuss your next project!

## License

[MIT License](LICENSE)
