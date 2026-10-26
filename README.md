# Hameed Latif Hospital Laboratories

Full-stack medical diagnostic laboratory platform featuring an Angular 22 frontend and a secure .NET 8 Clean Architecture AI ChatBot API powered by Google Gemini.

---

## 🏗️ Architecture Overview

- **Frontend (`frontend/`)**: Angular 22 (Zoneless architecture with Signals, Standalone Components, responsive Tailwind-free modern CSS).
- **Backend (`backend/`)**: .NET 8 (C# 12) Web API with Clean Architecture:
  - **API Layer**: Controllers, IP-based Rate Limiting (Fixed window: 30 req/min), FluentValidation, global RFC 7807 Exception Handling middleware, Swagger/OpenAPI.
  - **Application Layer**: Grounded Chat Service, Dynamic Prompt Builder, Response Sanitizer, DTOs, and Interfaces.
  - **Domain Layer**: Diagnostic Test entities, FAQ entities, Repository contracts.
  - **Infrastructure Layer**: JSON Test Catalog Repository (750+ verified lab tests extracted from official price lists), JSON FAQ Repository, In-memory conversation session store (6-message rolling context, 30-min TTL), and resilient `GeminiClient`.
  - **Tests Layer**: xUnit test suite (22 unit tests covering PromptBuilder, ResponseSanitizer, Repositories, and Session Store).

---

## 🔐 Security & Gemini API Configuration

**CRITICAL:** The Google Gemini API key is **never** committed to Git or exposed in the frontend bundle. The frontend communicates exclusively with the backend proxy endpoint.

### 1. Environment Variable (Recommended for Production & Cloud)
Set the `GEMINI_API_KEY` environment variable in your hosting platform (Docker, Render, Railway, Azure, etc.):
```bash
export GEMINI_API_KEY="your_actual_gemini_api_key_here"
```

### 2. Local Development Configuration
For local development, copy the `.env.example` or create `backend/src/HammadLatifLab.ChatBot.Api/appsettings.Development.json` (which is excluded in `.gitignore`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "Gemini": {
    "ApiKey": "your_local_gemini_api_key"
  }
}
```

---

## 🚀 Getting Started Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v20+ recommended) & npm

### 1. Run the Backend API (.NET 8)
```bash
cd backend/src/HammadLatifLab.ChatBot.Api
dotnet run --urls="http://localhost:5153"
```
The API is available at `http://localhost:5153` (Swagger UI at `http://localhost:5153/swagger`, Health at `http://localhost:5153/health`).

### 2. Run the Frontend (Angular 22)
```bash
cd frontend
npm install
npm start -- --port 4300
```
Open `http://localhost:4300` in your browser.

---

## 🧪 Testing & Verification

### Run Backend Unit Tests
```bash
cd backend
dotnet test HammadLatifLab.ChatBot.sln
```

### Build Production Frontend Bundle
```bash
cd frontend
npm run build
```
Build output will be located in `frontend/dist/hameed-latif-angular`.
