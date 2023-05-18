using BlazorTask.Data;

namespace BlazorTask.Services.Interfaces;

public interface IProjectService
{
    Task CreateAsync();
    Task<List<Project>> GetAllAsync();
}