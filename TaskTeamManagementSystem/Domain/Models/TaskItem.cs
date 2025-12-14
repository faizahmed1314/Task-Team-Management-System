namespace TaskTeamManagementSystem.Domain.Models;

public class TaskItem
{
    public int Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public TaskStatus Status { get; set; }
    
    public int AssignedToUserId { get; set; }
    
    public int CreatedByUserId { get; set; }
    
    public int TeamId { get; set; }
    
    public DateTime DueDate { get; set; }

    public TaskItem() { }

    public TaskItem(int id, string title, string description, TaskStatus status, 
        int assignedToUserId, int createdByUserId, int teamId, DateTime dueDate)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        AssignedToUserId = assignedToUserId;
        CreatedByUserId = createdByUserId;
        TeamId = teamId;
        DueDate = dueDate;
    }
}

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
