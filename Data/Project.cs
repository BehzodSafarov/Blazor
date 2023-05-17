namespace BlazorTask.Data;

public class Project
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? RepoUrl { get; set; }
    public string? LogoUrl { get; set; }
}