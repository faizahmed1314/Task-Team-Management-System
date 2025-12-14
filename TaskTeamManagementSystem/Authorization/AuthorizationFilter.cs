using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Authorization
{
    public static class AuthorizationFilter
    {
        public static async Task<IResult> AuthorizeAsync(
            HttpContext context,
            IAuthorizationService authService,
            Func<User, Task<IResult>> handler,
            params Role[] requiredRoles)
        {
            var user = await authService.GetCurrentUserAsync(context);

            if (user == null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized",
                    detail: "User authentication required. Please provide X-User-Id header."
                );
            }

            if (!authService.IsAuthorized(user, requiredRoles))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: $"User does not have the required role(s). Required: {string.Join(", ", requiredRoles)}"
                );
            }

            return await handler(user);
        }

        public static async Task<IResult> AuthorizeTaskUpdateAsync(
            HttpContext context,
            IAuthorizationService authService,
            int taskId,
            Func<User, Task<IResult>> handler)
        {
            var user = await authService.GetCurrentUserAsync(context);

            if (user == null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized",
                    detail: "User authentication required. Please provide X-User-Id header."
                );
            }

            var canUpdate = await authService.CanUpdateTaskAsync(user, taskId);

            if (!canUpdate)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You do not have permission to update this task"
                );
            }

            return await handler(user);
        }
    }
}
