# <img width="64" height="64" alt="Pititi-South" src="https://github.com/user-attachments/assets/4bd5e48f-c2cc-435e-a69d-be37d313741b" /> Pititi-Bot

![Beta](https://img.shields.io/badge/status-BETA-orange?style=for-the-badge)

**HIHI IS OF PITITI!!**

A Space Station 14 Discord bot by Vox for Vox. It's very tailored to my specific needs but hey, if you wish to use it go ahead!

> **⚠️ BETA SOFTWARE:** This bot is currently in beta. Features may change, and you may encounter bugs. Use at your own risk!

## About

Pititi is a small, enthusiastic VOX from Space Station 14 with a very basic understanding of English. This bot brings his charming personality to Discord!

## Features

### Commands

#### `/ping`
Check if Pititi is alive and well!
- **Permission Required:** Manage Roles
- **Response:** "HIHI IS OF PITITI!!"

#### `/coinflip`
Pititi flips a coin for you!
- **Response:** Either HEADINGS or TAILINGS (with enthusiasm!)

#### `/dice`
Pititi throws dice for you!
- **Options:**
  - `choice`: D4, D6, D8, D10, D12, or D20
  - `count`: Number of dice to roll (1-10, defaults to 1)
- **Features:**
  - Single die rolls show individual results with flavor text
  - Multiple dice show all rolls and a total
  - Special messages for max rolls and critical failures
  - Mentions who Pititi is rolling for

#### `/landmine`
Pititi's boom box game! Place a landmine that explodes after a random number of messages.

**Actions:**
- **Place** - Plants a boom box that will explode after 1-250 random messages
  - Only one landmine per channel
  - Pititi keeps the countdown secret!
  - Permission Required: Manage Messages
- **Remove** - Safely removes the boom box
  - Shows how many messages were remaining
  - Permission Required: Manage Messages
- **Status** - Check boom box status (ephemeral)
  - Shows initial countdown
  - Shows messages passed
  - Shows remaining messages
  - Only visible to you!
  - Permission Required: Manage Messages

**Features:**
- Mentions who stepped on the boom box
- Persistent storage using SQLite - survives bot restarts!

#### `/server`
A built-in SS14 monitor (currently in testing)

#### `/report`
Report bugs or request features for configured projects via GitHub!

**Parameters:**
- **type** - Choose between "Bug Report" or "Feature Request"
- **repository** - Select which project to report to (dynamically filtered by server)

**Features:**
- Opens an interactive modal form for detailed reports
- **Bug Reports include:**
  - Title
  - What happened?
  - Expected behavior (optional)
  - Screenshot URL (optional)
- **Feature Requests include:**
  - Title
  - Description
  - Why is this useful? (optional)
  - Mockup/Example URL (optional)
- Creates GitHub issues automatically with proper formatting
- Issues are prefixed with `Bug:` or `Feature:` for easy identification
- Includes reporter information (Discord username, ID, server)
- Auto-applies labels if they exist (bug/enhancement/user-reported)
- Server whitelist support - restrict which Discord servers can report to specific repositories

**How to add screenshots:**
1. Upload your image to Discord
2. Right-click the image → Copy Link
3. Paste the link in the "Image URL" field
4. The image will be embedded in the GitHub issue

## Technical Details

### Built With

- **C# / .NET 8.0**
- **Discord.Net** (v3.18.0)
- **Microsoft.Data.Sqlite** (v8.0.0)
- **Octokit** (v14.0.0) - GitHub API integration
- **DotNetEnv** (v3.1.1) - Environment variable support
- **Docker**

### Architecture

- **Modules:** Slash command handlers (`/Modules`)
- **Services:** Business logic and state management (`/Services`)
  - `LandmineService` - Manages boom box game state
  - `SS14StatusService` - Monitors Space Station 14 servers
  - `GitHubService` - Handles GitHub API interactions for issue creation
- **Database:** SQLite database stored in `/Databases` folder
- **Configuration:** Supports both `appsettings.json` and `.env` files

### Database

Pititi uses SQLite to remember important things (like where he placed boom boxes):
- **Database location:** `Databases/landmines.db`
- Automatically created on first run
- Survives bot restarts

## Deployment

### Using Docker Compose

1. Clone the repository:
```bash
git clone https://github.com/yourusername/Pititi-Bot.git
cd Pititi-Bot
```

2. Copy `.env.example` to `.env` and configure:
```bash
cp .env.example .env
```

Edit `.env` with your credentials:
```env
# Discord Bot Token
DISCORD_TOKEN=your_discord_bot_token_here

# GitHub Integration (Optional - for /report command)
GITHUB_TOKEN=ghp_your_github_token_here

# Repository Configuration
GitHub__Repositories__pititi-bot__Owner=ReboundQ3
GitHub__Repositories__pititi-bot__Name=Pititi-Bot
GitHub__Repositories__pititi-bot__DisplayName=Pititi Bot

# Server Whitelists (Optional - restrict which servers can report)
# REPO_PITITI-BOT_WHITELIST=1234567890123456789
```

3. Run with Docker Compose:
```bash
docker-compose up -d
```

### Using Docker

```bash
docker build -t pititi-bot .
docker run -e DISCORD_TOKEN=your_token_here pititi-bot
```

### Running Locally

1. Install .NET 8.0 SDK
2. Clone the repository
3. Create `appsettings.json` in `PititiBot/` folder:
```json
{
  "Discord": {
    "Token": "your_discord_bot_token_here"
  }
}
```

4. Run the bot:
```bash
cd PititiBot
dotnet restore
dotnet build
dotnet run
```

## Configuration

The bot supports configuration via `.env` files or `appsettings.json`:

### Discord Configuration
- `DISCORD_TOKEN` - Your Discord bot token (required)

### GitHub Integration (Optional)
Configure GitHub issue reporting:

1. **Create a GitHub Personal Access Token:**
   - Go to https://github.com/settings/tokens
   - Click "Generate new token (classic)"
   - Select scope: `repo` (Full control of private repositories)
   - Copy the token

2. **Configure repositories in `.env`:**
```env
GITHUB_TOKEN=ghp_your_token_here

# Add repositories
GitHub__Repositories__my-project__Owner=YourUsername
GitHub__Repositories__my-project__Name=repo-name
GitHub__Repositories__my-project__DisplayName=My Project

# Optional: Restrict by server ID
REPO_MY-PROJECT_WHITELIST=1234567890123456789,9876543210987654321
```

3. **Server Whitelists:**
   - Control which Discord servers can report to each repository
   - Format: `REPO_<REPO_KEY>_WHITELIST=server_id1,server_id2`
   - If not set, all servers can report to that repository
   - Get server IDs: Enable Developer Mode in Discord → Right-click server → Copy Server ID

**Examples:**
```env
# Pititi Bot - accessible from all servers (no whitelist)
# REPO_PITITI-BOT_WHITELIST=

# Sector Vestige - only from specific server
REPO_SECTOR-VESTIGE_WHITELIST=1234567890123456789

# Project X - accessible from multiple servers
REPO_PROJECT-X_WHITELIST=111111111111111111,222222222222222222
```

See `appsettings.template.json` for JSON-based configuration example.

## CI/CD

The project includes a GitHub Actions workflow that:
- Builds Docker images automatically
- Pushes to GitHub Container Registry
- Tags images with branch name, SHA, and `latest`

## Development

### Project Structure

```
PititiBot/
├── Modules/                    # Discord slash command modules
│   ├── CoinFlipModule.cs
│   ├── DiceModule.cs
│   ├── EightballModule.cs
│   ├── GitHubIssueModule.cs   # GitHub issue reporting
│   ├── HelpModule.cs
│   ├── LandmineModule.cs
│   ├── PingModule.cs
│   └── SS14StatusModule.cs
├── Services/                   # Business logic services
│   ├── GitHubService.cs       # GitHub API integration
│   ├── LandmineService.cs
│   └── SS14StatusService.cs
├── Databases/                  # SQLite databases (gitignored)
│   └── landmines.db
├── Program.cs                  # Application entry point
├── .env.example               # Environment configuration template
└── appsettings.template.json  # JSON configuration template
```

### Adding New Commands

1. Create a new module in the `Modules/` folder
2. Inherit from `InteractionModuleBase<SocketInteractionContext>`
3. Add namespace: `namespace PititiBot.Modules;`
4. Use `[SlashCommand]` attribute
5. Write responses in Pititi's voice!

**Example:**

```csharp
using Discord;
using Discord.Interactions;

namespace PititiBot.Modules;

public class YourModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("yourcommand", "Command description")]
    public async Task HandleCommand()
    {
        await RespondAsync("PITITI DO THING!! YAYA!");
    }
}
```

## Contributing

Feel free to submit issues or pull requests! Make sure to maintain Pititi's enthusiastic personality in all responses.

## License

This project is created by Vox for Vox 🦎

---

*"HIHI!! Pititi help you! Is fun yes?"*
