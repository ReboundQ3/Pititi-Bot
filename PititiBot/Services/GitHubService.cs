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

        var newIssue = new NewIssue(title)
        {
            Body = body
        };

        // Add labels based on issue type
        var label = issueType.ToLower() switch
        {
            "bug" => "bug",
            "feature" => "enhancement",
            "question" => "question",
            _ => "user-reported"
        };
        newIssue.Labels.Add(label);
        newIssue.Labels.Add("user-reported");

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
