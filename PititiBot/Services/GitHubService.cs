using Octokit;

namespace PititiBot.Services;

public class GitHubService
{
    private readonly GitHubClient _client;
    private readonly Dictionary<string, RepositoryConfig> _repositories;

    public GitHubService(string personalAccessToken, Dictionary<string, RepositoryConfig> repositories)
    {
        _client = new GitHubClient(new ProductHeaderValue("PititiBot"))
        {
            Credentials = new Credentials(personalAccessToken)
        };
        _repositories = repositories;
    }

    public async Task<Issue> CreateIssueAsync(string repositoryKey, string title, string body, string issueType)
    {
        if (!_repositories.TryGetValue(repositoryKey, out var repo))
        {
            throw new ArgumentException($"Repository '{repositoryKey}' not found in configuration");
        }

        // Prefix title with issue type
        var prefix = issueType.ToLower() switch
        {
            "bug" => "Bug:",
            "feature" => "Feature request:",
            "question" => "Question:",
            _ => ""
        };
        var issueTitle = string.IsNullOrEmpty(prefix) ? title : $"{prefix} {title}";

        var newIssue = new NewIssue(issueTitle)
        {
            Body = body
        };

        // Try to add labels if they exist, but don't fail if we can't
        try
        {
            var label = issueType.ToLower() switch
            {
                "bug" => "bug",
                "feature" => "feature request",
                "question" => "question",
                _ => "user-reported"
            };

            // Check if labels exist first
            var existingLabels = await _client.Issue.Labels.GetAllForRepository(repo.Owner, repo.Name);
            var labelNames = existingLabels.Select(l => l.Name).ToList();

            if (labelNames.Contains(label))
                newIssue.Labels.Add(label);
            if (labelNames.Contains("user-reported"))
                newIssue.Labels.Add("user-reported");
        }
        catch
        {
            // Silently ignore label errors - labels are nice to have, not required
        }

        var issue = await _client.Issue.Create(repo.Owner, repo.Name, newIssue);
        return issue;
    }

    public IEnumerable<string> GetRepositoryKeys()
    {
        return _repositories.Keys;
    }

    public string GetRepositoryDisplayName(string key)
    {
        return _repositories.TryGetValue(key, out var repo) ? repo.DisplayName : key;
    }

    public bool IsServerAllowed(string repositoryKey, ulong? guildId)
    {
        if (!_repositories.TryGetValue(repositoryKey, out var repo))
            return false;

        // If no whitelist configured, allow all
        if (repo.ServerWhitelist == null || repo.ServerWhitelist.Count == 0)
            return true;

        // If no guild (DM), deny if whitelist exists
        if (guildId == null)
            return false;

        // Check if server is in whitelist
        return repo.ServerWhitelist.Contains(guildId.Value);
    }

    public IEnumerable<string> GetAllowedRepositories(ulong? guildId)
    {
        return _repositories
            .Where(kvp => IsServerAllowed(kvp.Key, guildId))
            .Select(kvp => kvp.Key);
    }
}

public class RepositoryConfig
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public HashSet<ulong>? ServerWhitelist { get; set; } = null;
}
