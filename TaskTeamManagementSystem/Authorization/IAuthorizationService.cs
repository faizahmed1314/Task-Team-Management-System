using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Authorization
{
    public interface IAuthorizationService
    {
        Task<User?> GetCurrentUserAsync(HttpContext context);
        bool IsAuthorized(User? user, params Role[] requiredRoles);
        Task<bool> CanUpdateTaskAsync(User? user, int taskId);
    }
}
