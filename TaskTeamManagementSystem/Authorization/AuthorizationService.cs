using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IApplicationDbContext _context;

        public AuthorizationService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                return null;
            }

            if (!int.TryParse(userIdHeader, out var userId))
            {
                return null;
            }

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public bool IsAuthorized(User? user, params Role[] requiredRoles)
        {
            if (user == null)
            {
                return false;
            }

            return requiredRoles.Contains(user.Role);
        }

        public async Task<bool> CanUpdateTaskAsync(User? user, int taskId)
        {
            if (user == null)
            {
                return false;
            }

            if (user.Role == Role.Admin || user.Role == Role.Manager)
            {
                return true;
            }

            if (user.Role == Role.Employee)
            {
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedToUserId == user.Id);
                return task != null;
            }

            return false;
        }
    }
}
