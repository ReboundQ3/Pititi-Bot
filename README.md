<img width="64" height="64" alt="Pititi-South" src="https://github.com/user-attachments/assets/4bd5e48f-c2cc-435e-a69d-be37d313741b" /> Pititi-Bot

A Space Station 14 Discord bot by Vox for Vox
Its very tailored to my spesific needs but hey, if you wish to use it go ahead!

HIHI IS OF PITITI!!

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

#### `/Server`
A built in SS14 monitor, but this is in testing still 

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

## Technical Details

### Built With
- C# / .NET 8.0
- Discord.Net (v3.18.0)
- Microsoft.Data.Sqlite (v8.0.0)
- Docker

### Architecture
- **Modules**: Slash command handlers (`/Modules`)
- **Services**: Business logic and state management (`/Services`)
- **Database**: SQLite database stored in `/Databases` folder
- **Configuration**: Supports both `appsettings.json` and environment variables

### Database
Pititi uses SQLite to remember important things (like where he placed boom boxes):
- Database location: `Databases/landmines.db`
- Automatically created on first run
- Survives bot restarts

## Deployment

### Using Docker Compose

1. Clone the repository:
```bash
git clone https://github.com/yourusername/Pititi-Bot.git
cd Pititi-Bot
```

2. Create a `.env` file with your Discord token:
```env
DISCORD_TOKEN=your_discord_bot_token_here
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

The bot supports configuration via:
- `appsettings.json` (see `appsettings.template.json` for example)
- Environment variables: `DISCORD_TOKEN`

## CI/CD

The project includes GitHub Actions workflow that:
- Builds Docker images automatically
- Pushes to GitHub Container Registry
- Tags images with branch name, SHA, and `latest`

## Development

### Project Structure
```
PititiBot/
├── Modules/              # Discord slash command modules
│   ├── CoinFlipModule.cs
│   ├── DiceModule.cs
│   ├── LandmineModule.cs
│   └── PingModule.cs
├── Services/             # Business logic services
│   └── LandmineService.cs
├── Databases/            # SQLite databases (gitignored)
│   └── landmines.db
└── Program.cs            # Application entry point
```

### Adding New Commands

1. Create a new module in `Modules/` folder
2. Inherit from `InteractionModuleBase<SocketInteractionContext>`
3. Add namespace: `namespace PititiBot.Modules;`
4. Use `[SlashCommand]` attribute
5. Write responses in Pititi's voice!

Example:
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
