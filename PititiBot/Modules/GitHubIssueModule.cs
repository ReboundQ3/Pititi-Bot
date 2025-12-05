using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PititiBot.Services;

namespace PititiBot.Modules;

public class GitHubIssueModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GitHubService _githubService;

    public GitHubIssueModule(GitHubService githubService)
    {
        _githubService = githubService;
    }

    [SlashCommand("report", "Report a bug or request a feature")]
    public async Task ReportCommand(
        [Choice("Pititi Bot", "pititi-bot"), Choice("Sector Vestige", "sector-vestige")] string repository)
    {
        var modal = new ModalBuilder()
            .WithTitle($"Report Issue - {_githubService.GetRepositoryDisplayName(repository)}")
            .WithCustomId($"github_issue_modal:{repository}")
            .AddTextInput("Title", "issue_title", TextInputStyle.Short,
                "Brief description of the issue", 1, 100, true)
            .AddTextInput("Description", "issue_description", TextInputStyle.Paragraph,
                "Detailed description. What happened? What did you expect?", 10, 2000, true)
            .AddTextInput("Type", "issue_type", TextInputStyle.Short,
                "bug, feature, or question", 3, 20, true, "bug")
            .Build();

        await RespondWithModalAsync(modal);
    }

    [ModalInteraction("github_issue_modal:*")]
    public async Task HandleModalSubmit(string repository, IssueModal modal)
    {
        await DeferAsync(ephemeral: true);

        try
        {
            // Validate issue type
            var validTypes = new[] { "bug", "feature", "question" };
            var issueType = modal.IssueType.ToLower().Trim();
            if (!validTypes.Contains(issueType))
            {
                await FollowupAsync("Invalid issue type. Please use 'bug', 'feature', or 'question'.", ephemeral: true);
                return;
            }

            // Create the issue body with metadata
            var issueBody = $"{modal.Description}\n\n---\n" +
                           $"**Reported by:** {Context.User.Username} (Discord ID: {Context.User.Id})\n" +
                           $"**Reported via:** Pititi Bot";

            var issue = await _githubService.CreateIssueAsync(repository, modal.Title, issueBody, issueType);

            var embed = new EmbedBuilder()
                .WithTitle("Issue Created Successfully!")
                .WithDescription($"Your {issueType} report has been submitted.")
                .WithColor(Color.Green)
                .AddField("Title", modal.Title)
                .AddField("Issue Number", $"#{issue.Number}")
                .AddField("URL", issue.HtmlUrl)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("YAYA!! Thank you for the report!")
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Error creating GitHub issue: {ex.Message}");
            await FollowupAsync($"Failed to create issue: {ex.Message}\nPlease contact the bot administrator.", ephemeral: true);
        }
    }
}

public class IssueModal : IModal
{
    public string Title => "Issue Report";

    [InputLabel("Title")]
    [ModalTextInput("issue_title")]
    public string IssueTitle { get; set; } = string.Empty;

    [InputLabel("Description")]
    [ModalTextInput("issue_description")]
    public string Description { get; set; } = string.Empty;

    [InputLabel("Type")]
    [ModalTextInput("issue_type")]
    public string IssueType { get; set; } = string.Empty;
}
