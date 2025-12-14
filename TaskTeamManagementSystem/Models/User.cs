namespace TaskTeamManagementSystem.Models;

public class User
{
    public int Id { get; set; }
    
    public required string FullName { get; set; }
    
    public required string Email { get; set; }
    
    public required string Password { get; set; }
    
    public Role Role { get; set; }

    // Parameterless constructor for EF Core
    public User() { }

    // Constructor for initial data seeding
    public User( string fullName, string email, string password, Role role)
    {
        FullName = fullName;
        Email = email;
        Password = password;
        Role = role;
    }
}

public enum Role
{
    Admin,
    Manager,
    Employee
}