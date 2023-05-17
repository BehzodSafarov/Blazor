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
    public async Task<Data.Project> CreateAsync(string ownerName, string repoName)
    {
        _logger.LogInformation($"Started creating project of OwnerName {ownerName} and RepoName {repoName}");
        try
        {
            var github = new GitHubClient(new ProductHeaderValue("BlazorTask"));

            var repository = await github.Repository.Get(ownerName, repoName);

            if(repository is null)
            {
                _logger.LogInformation("This repository didn't found");
                return new();
            }
            
            var oldProject = _context.Projects!.FirstOrDefault(x => x.Name == repository.FullName && x.RepoUrl == repository.HtmlUrl);

            if(oldProject is not null)
            {
                _logger.LogInformation($"This project already exist");
                return new();
            }
            var newProject = new Data.Project
            {
                Name = repository.FullName,
                Description = repository.Description,
                LogoUrl = repository.Owner.AvatarUrl,
                RepoUrl = repository.HtmlUrl
            };

            var savedProject = await _context.Projects!.AddAsync(newProject);
            await _context.SaveChangesAsync();

            if(savedProject is null)
            {
                _logger.LogInformation($"Project is not created");
                return new();
            }

            return savedProject.Entity;
        }
        catch (System.Exception e)
        {
            _logger.LogInformation("Failed project while saving in database");
            return null;
        }
    }

    public async Task CreateDefaultProjectsAsync()
    {
        _logger.LogInformation($"Started creating default projects");
        var gitHubProject = new Dictionary<string, string>
        {
            { "Spoon-Knife", "octocat" },
            { "hello-world", "octocat" },
            { "octocat.github.io", "octocat" },
            { "test-repo", "octocat" },
            { "gitignore", "github" },
            { "hub", "github" },
            { "bootstrap", "twbs" },
            { "freeCodeCamp", "freeCodeCamp" },
            { "VSCode", "microsoft" },
            { "angular", "angular" }
        };
        try
        {
           foreach (var project in gitHubProject)
           {
             await CreateAsync(project.Value, project.Key);
           }
        }
        catch (System.Exception e)
        {
            _logger.LogInformation(e.Message);
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteAsync(Guid? id)
    {
        try
        {
            if(Guid.Empty == id)
            {
                _logger.LogInformation($"This Id is not available id = {id}");
                return;
            }
            
            var project = _context.Projects!.FirstOrDefault(x => x.Id == id);

            if(project is null)
            {
                _logger.LogInformation($"This project didn't found with id = {id}");
                return;
            }

            _context.Remove(project);
            await _context.SaveChangesAsync();
        }
        catch (System.Exception e)
        {
            _logger.LogInformation($"Failed removing project with Id {id}");
            throw new Exception(e.Message);
        }
    }

    public async Task<List<Data.Project>> GetAllAsync()
    => _context.Projects!.ToList();
}