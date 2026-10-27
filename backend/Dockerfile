# -----------------------------------------------------------------------------
# Stage 1: Build & Publish
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files for optimal layer caching
COPY HammadLatifLab.ChatBot.sln ./
COPY src/HammadLatifLab.ChatBot.Api/*.csproj src/HammadLatifLab.ChatBot.Api/
COPY src/HammadLatifLab.ChatBot.Application/*.csproj src/HammadLatifLab.ChatBot.Application/
COPY src/HammadLatifLab.ChatBot.Domain/*.csproj src/HammadLatifLab.ChatBot.Domain/
COPY src/HammadLatifLab.ChatBot.Infrastructure/*.csproj src/HammadLatifLab.ChatBot.Infrastructure/
COPY tests/HammadLatifLab.ChatBot.Tests/*.csproj tests/HammadLatifLab.ChatBot.Tests/

# Restore dependencies
RUN dotnet restore HammadLatifLab.ChatBot.sln

# Copy source code and test files
COPY src/ src/
COPY tests/ tests/

# Publish Release build
RUN dotnet publish src/HammadLatifLab.ChatBot.Api/HammadLatifLab.ChatBot.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# Stage 2: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Set environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV PORT=8080

# Copy published application and lab data (faqs.json & tests.json)
COPY --from=build /app/publish ./

# Expose standard and Render ports
EXPOSE 8080
EXPOSE 10000

# Start ASP.NET Core API
ENTRYPOINT ["dotnet", "HammadLatifLab.ChatBot.Api.dll"]
