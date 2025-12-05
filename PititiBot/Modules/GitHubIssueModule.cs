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
        [Summary("type", "What type of report?")]
        [Choice("Bug Report", "bug")]
        [Choice("Feature Request", "feature")]
        string type,
        [Summary("repository", "Which project to report to")]
        [Autocomplete(typeof(RepositoryAutocompleteHandler))]
        string repository)
    {
        // Validate repository access
        if (!_githubService.IsServerAllowed(repository, Context.Guild?.Id))
        {
            await RespondAsync("You are not allowed to report to this project from this server.", ephemeral: true);
            return;
        }

        // Build modal based on type
        if (type == "bug")
        {
            var modal = new ModalBuilder()
                .WithTitle($"Bug Report - {_githubService.GetRepositoryDisplayName(repository)}")
                .WithCustomId($"bug_modal:{repository}")
                .AddTextInput("Title", "bug_title", TextInputStyle.Short,
                    "Brief description of the bug", 1, 100, true)
                .AddTextInput("What happened?", "bug_description", TextInputStyle.Paragraph,
                    "Describe what went wrong", 10, 2000, true)
                .AddTextInput("Expected behavior", "bug_expected", TextInputStyle.Paragraph,
                    "What should have happened instead?", 10, 1000, false)
                .AddTextInput("Image URL (optional)", "bug_image", TextInputStyle.Short,
                    "Paste a link to a screenshot (upload to Discord first, then copy link)", 0, 500, false)
                .Build();

            await RespondWithModalAsync(modal);
        }
        else // feature
        {
            var modal = new ModalBuilder()
                .WithTitle($"Feature Request - {_githubService.GetRepositoryDisplayName(repository)}")
                .WithCustomId($"feature_modal:{repository}")
                .AddTextInput("Title", "feature_title", TextInputStyle.Short,
                    "Brief description of the feature", 1, 100, true)
                .AddTextInput("Description", "feature_description", TextInputStyle.Paragraph,
                    "Describe what you'd like to see added", 10, 2000, true)
                .AddTextInput("Why is this useful?", "feature_reason", TextInputStyle.Paragraph,
                    "Explain the benefit of this feature", 10, 1000, false)
                .AddTextInput("Image URL (optional)", "feature_image", TextInputStyle.Short,
                    "Paste a link to a mockup/example (upload to Discord first, then copy link)", 0, 500, false)
                .Build();

            await RespondWithModalAsync(modal);
        }
    }

    [ModalInteraction("bug_modal:*")]
    public async Task HandleBugModalSubmit(string repository, BugModal modal)
    {
        await DeferAsync(ephemeral: true);

        try
        {
            // Build issue body
            var issueBody = $"## What happened?\n{modal.Description}\n\n";

            if (!string.IsNullOrWhiteSpace(modal.ExpectedBehavior))
            {
                issueBody += $"## Expected behavior\n{modal.ExpectedBehavior}\n\n";
            }

            if (!string.IsNullOrWhiteSpace(modal.ImageUrl))
            {
                issueBody += $"## Screenshot\n![Screenshot]({modal.ImageUrl.Trim()})\n\n";
            }

            issueBody += $"---\n**Reported by:** {Context.User.Username} (Discord ID: {Context.User.Id})\n" +
                        $"**Server:** {Context.Guild?.Name ?? "DM"}\n" +
                        $"**Reported via:** Pititi Bot";

            var issue = await _githubService.CreateIssueAsync(repository, modal.BugTitle, issueBody, "bug");

            var embed = new EmbedBuilder()
                .WithTitle("🐛 Bug Report Created!")
                .WithDescription("Your bug report has been submitted. Thank you!")
                .WithColor(Color.Red)
                .AddField("Title", modal.BugTitle)
                .AddField("Issue Number", $"#{issue.Number}")
                .AddField("URL", issue.HtmlUrl)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("YAYA!! We'll look into it!")
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Error creating GitHub bug: {ex.Message}");
            await FollowupAsync($"Failed to create bug report: {ex.Message}\nPlease contact the bot administrator.", ephemeral: true);
        }
    }

    [ModalInteraction("feature_modal:*")]
    public async Task HandleFeatureModalSubmit(string repository, FeatureModal modal)
    {
        await DeferAsync(ephemeral: true);

        try
        {
            // Build issue body
            var issueBody = $"## Description\n{modal.Description}\n\n";

            if (!string.IsNullOrWhiteSpace(modal.Reason))
            {
                issueBody += $"## Why is this useful?\n{modal.Reason}\n\n";
            }

            if (!string.IsNullOrWhiteSpace(modal.ImageUrl))
            {
                issueBody += $"## Mockup/Example\n![Example]({modal.ImageUrl.Trim()})\n\n";
            }

            issueBody += $"---\n**Requested by:** {Context.User.Username} (Discord ID: {Context.User.Id})\n" +
                        $"**Server:** {Context.Guild?.Name ?? "DM"}\n" +
                        $"**Requested via:** Pititi Bot";

            var issue = await _githubService.CreateIssueAsync(repository, modal.FeatureTitle, issueBody, "feature");

            var embed = new EmbedBuilder()
                .WithTitle("✨ Feature Request Created!")
                .WithDescription("Your feature request has been submitted. Thank you!")
                .WithColor(Color.Blue)
                .AddField("Title", modal.FeatureTitle)
                .AddField("Issue Number", $"#{issue.Number}")
                .AddField("URL", issue.HtmlUrl)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithFooter("YAYA!! We'll consider it!")
                .Build();

            await FollowupAsync(embed: embed, ephemeral: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"#> Error creating GitHub feature: {ex.Message}");
            await FollowupAsync($"Failed to create feature request: {ex.Message}\nPlease contact the bot administrator.", ephemeral: true);
        }
    }
}

// Autocomplete handler to dynamically show repository options based on server
public class RepositoryAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        var githubService = services.GetService(typeof(GitHubService)) as GitHubService;
        if (githubService == null)
            return AutocompletionResult.FromSuccess(Array.Empty<AutocompleteResult>());

        var allowedRepos = githubService.GetAllowedRepositories(context.Guild?.Id);
        var results = allowedRepos
            .Select(key => new AutocompleteResult(githubService.GetRepositoryDisplayName(key), key))
            .ToList();

        return AutocompletionResult.FromSuccess(results);
    }
}

public class BugModal : IModal
{
    public string Title => "Bug Report";

    [InputLabel("Title")]
    [ModalTextInput("bug_title")]
    public string BugTitle { get; set; } = string.Empty;

    [InputLabel("What happened?")]
    [ModalTextInput("bug_description")]
    public string Description { get; set; } = string.Empty;

    [InputLabel("Expected behavior")]
    [ModalTextInput("bug_expected")]
    public string ExpectedBehavior { get; set; } = string.Empty;

    [InputLabel("Image URL (optional)")]
    [ModalTextInput("bug_image")]
    public string ImageUrl { get; set; } = string.Empty;
}

public class FeatureModal : IModal
{
    public string Title => "Feature Request";

    [InputLabel("Title")]
    [ModalTextInput("feature_title")]
    public string FeatureTitle { get; set; } = string.Empty;

    [InputLabel("Description")]
    [ModalTextInput("feature_description")]
    public string Description { get; set; } = string.Empty;

    [InputLabel("Why is this useful?")]
    [ModalTextInput("feature_reason")]
    public string Reason { get; set; } = string.Empty;

    [InputLabel("Image URL (optional)")]
    [ModalTextInput("feature_image")]
    public string ImageUrl { get; set; } = string.Empty;
}
