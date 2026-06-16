# <img width="64" height="64" alt="Pititi-South" src="https://github.com/user-attachments/assets/4bd5e48f-c2cc-435e-a69d-be37d313741b" /> Pititi-Bot

![Beta](https://img.shields.io/badge/status-BETA-orange?style=for-the-badge)

**HIHI IS OF PITITI!!**

A Space Station 14 Discord bot starring Pititi, a small enthusiastic VOX with a shaky grasp of English. Built by Vox for Vox — tailored to my needs, but you're welcome to use it.

> ⚠️ **Beta software** — features may change and bugs may bite.

## Commands

| Command | What it does | Notes |
|---|---|---|
| `/ping` | Check Pititi is alive | Needs *Manage Roles* |
| `/coinflip` | Flip a coin | |
| `/dice` | Roll dice (D4–D100, 1–10 at once) | |
| `/8ball` | Ask the magic eightball | |
| `/landmine` | Plant "boom boxes" that explode after a random number of messages | `place` / `remove` / `clearall` / `status` · needs *Manage Messages* |
| `/leaderboard` | Server ranking of who stepped on the most boom boxes | Top 10, MEGA BOOMs count per mine |
| `/server` | SS14 server status + round notifications | `status` / `subscribe` / `unsubscribe` / `config` (admin) |
| `/report` | File a bug or feature request as a GitHub issue | Opens a modal; repos are server-whitelisted |

## Run it

The easy path is Docker Compose:

```bash
git clone https://github.com/ReboundQ3/Pititi-Bot.git
cd Pititi-Bot
cp .env.example .env   # then fill in your token(s)
docker compose up -d
```

SQLite databases persist in `./data` between restarts.

To run from source instead, install the **.NET 10 SDK** and:

```bash
cd PititiBot
dotnet run   # reads .env or appsettings.json
```

## Configuration

Set via `.env` (see `.env.example`) or `appsettings.json` (see `appsettings.template.json`).

| Variable | Required | Purpose |
|---|---|---|
| `DISCORD_TOKEN` | ✅ | Discord bot token |
| `GITHUB_TOKEN` | optional | PAT with `repo` scope, enables `/report` |

**GitHub repositories** for `/report` are configured per key:

```env
GitHub__Repositories__my-project__Owner=YourUsername
GitHub__Repositories__my-project__Name=repo-name
GitHub__Repositories__my-project__DisplayName=My Project

# Optional: restrict which Discord servers may report to this repo
REPO_MY-PROJECT_WHITELIST=server_id1,server_id2
```

If a repo has no whitelist, any server can report to it.

## Tech

C# / .NET 10 · [Discord.Net](https://github.com/discord-net/Discord.Net) · [Octokit](https://github.com/octokit/octokit.net) · SQLite · Docker.

- **`Modules/`** — slash command handlers
- **`Services/`** — game state, SS14 monitoring, GitHub integration
- **`Databases/`** — SQLite, auto-created on first run (gitignored)

CI builds a Docker image on every push to `main` and pushes it to GHCR.

To add a command, drop a new module in `Modules/` that extends `InteractionModuleBase<SocketInteractionContext>` and write the responses in Pititi's voice.

## License

Created by Vox for Vox 🦎

---

*"HIHI!! Pititi help you! Is fun yes?"*
