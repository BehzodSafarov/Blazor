using BlazorTask.Data;
using BlazorTask.Services.Interfaces;
using Octokit;

namespace BlazorTask.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task CreateAsync()
    {
        _logger.LogInformation("Started Creating 10 projects");

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Timer timer = new Timer(CancelOperation!, cancellationTokenSource, TimeSpan.FromMinutes(10), Timeout.InfiniteTimeSpan);

        var github = new GitHubClient(new ProductHeaderValue("Blazor"));

        try
        {
            var searchRequest = new SearchRepositoriesRequest
            {
                
                PerPage = 30,
                Topic = GetRandomName()
            };
            
            _logger.LogInformation($"Taking random repositories");
            var searchResult = await github.Search.SearchRepo(searchRequest);

            var repositories = searchResult.Items.Where(repo => !repo.Fork).Take(10);

            foreach (var repository in repositories)
            {
                var newProject = new Data.Project
                {
                    Id = Guid.NewGuid(),
                    Name = repository.FullName,
                    RepoUrl = repository.HtmlUrl,
                    LogoUrl = repository.Owner.AvatarUrl,
                    Description = repository.Description
                };
                
                _logger.LogInformation($"Saving repository in database with Name {repository.FullName}");
                await _context.AddAsync(newProject);
                await _context.SaveChangesAsync();
            }

        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error: {ex.Message}");
        }

        timer.Dispose();
        cancellationTokenSource.Dispose();
    }

    public static void CancelOperation(object state)
    {
        CancellationTokenSource cancellationTokenSource = (CancellationTokenSource)state;
        cancellationTokenSource.Cancel();
    }

    private string GetRandomName()
    {
        List<string> names = new List<string>
        {
            "Dotnet",
            "Java",
            "Phyton",
            "C++",
            "React"
        };

        Random random = new Random();

        int randomIndex = random.Next(names.Count);

        _logger.LogInformation($"Taked RandomName {names[randomIndex]}");

        return names[randomIndex];
     
    }

    public async Task<List<Data.Project>> GetAllAsync()
    => _context.Projects!.ToList();
}