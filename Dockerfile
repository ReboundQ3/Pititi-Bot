# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY PititiBot/*.csproj ./PititiBot/
RUN dotnet restore ./PititiBot/PititiBot.csproj

# Copy everything else and build
COPY PititiBot/. ./PititiBot/
WORKDIR /src/PititiBot
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Run the bot
ENTRYPOINT ["dotnet", "PititiBot.dll"]
