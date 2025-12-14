using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using System.Security.Claims;

namespace TaskTeamManagementSystem.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthorizationService(IApplicationDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext context)
        {
            // First try JWT token from Authorization header
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                
                if (!string.IsNullOrEmpty(token))
                {
                    var principal = _jwtTokenService.ValidateToken(token);
                    
                    if (principal != null)
                    {
                        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) 
                                        ?? principal.FindFirst("sub");
                        
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                        {
                            return await _context.Users
                                .FirstOrDefaultAsync(u => u.Id == userId);
                        }
                    }
                }
            }

            // Fallback to X-User-Id header for backward compatibility
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                if (int.TryParse(userIdHeader, out var userId))
                {
                    return await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId);
                }
            }

            return null;
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
