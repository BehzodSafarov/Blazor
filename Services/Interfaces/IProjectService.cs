using BlazorTask.Data;

namespace BlazorTask.Services.Interfaces;

public interface IProjectService
{
    Task<Project> CreateAsync(string ownerName, string repoName);
    Task DeleteAsync(Guid? id);
    Task<List<Project>> GetAllAsync();
    Task CreateDefaultProjectsAsync();
}