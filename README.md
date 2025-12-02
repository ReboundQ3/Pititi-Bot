# Pititi-Bot
A Space Station 14 Discord bot by Vox for Vox 🦎

HIHI IS OF PITITI!!

## About
Pititi is a small, enthusiastic VOX from Space Station 14 with a very basic understanding of English. This bot brings his charming personality to Discord!

## Features
- `/ping` - Check if Pititi is alive and well!

## Deployment

### On Your Hetzner Server

1. **Install Docker** (if not already installed):
```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

2. **Clone the repository**:
```bash
git clone https://github.com/YOUR_USERNAME/Pititi-Bot.git
cd Pititi-Bot
```

3. **Create `.env` file**:
```bash
cp .env.example .env
nano .env
```
Add your Discord bot token.

4. **Pull and run the bot**:
```bash
docker-compose pull
docker-compose up -d
```

5. **View logs**:
```bash
docker-compose logs -f
```

6. **Update the bot** (after pushing changes):
```bash
docker-compose pull
docker-compose up -d
```

## Development

### Local Setup
1. Install .NET 8 SDK
2. Copy `appsettings.json` template and add your bot token
3. Run with `dotnet run --project PititiBot`

### CI/CD
Pushes to `main` automatically build and push Docker images to GitHub Container Registry.

## Tech Stack
- .NET 8
- Discord.Net 3.18.0
- Docker
