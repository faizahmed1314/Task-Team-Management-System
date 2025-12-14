namespace TaskTeamManagementSystem.Domain.Models;

public class Team
{
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }

    public Team() { }

    public Team(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
