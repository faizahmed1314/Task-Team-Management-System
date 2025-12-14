using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tasks.GetTasks
{
    public record GetTasksResponse(
        List<TaskItem> Tasks,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetTasksEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/tasks",
                async (ISender sender, 
                       HttpContext context,
                       IAuthorizationService authService,
                       TaskStatus? status, 
                       int? assignedToUserId, 
                       int? teamId, 
                       DateTime? dueDateFrom, 
                       DateTime? dueDateTo,
                       string? sortBy,
                       bool sortDescending = false,
                       int pageNumber = 1,
                       int pageSize = 10) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var finalAssignedToUserId = user.Role == Role.Employee 
                                ? user.Id 
                                : assignedToUserId;

                            var query = new GetTasksQuery(
                                Status: status,
                                AssignedToUserId: finalAssignedToUserId,
                                TeamId: teamId,
                                DueDateFrom: dueDateFrom,
                                DueDateTo: dueDateTo,
                                SortBy: sortBy,
                                SortDescending: sortDescending,
                                PageNumber: pageNumber,
                                PageSize: pageSize
                            );

                            var result = await sender.Send(query);

                            var response = result.Adapt<GetTasksResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin, Role.Manager, Role.Employee
                    );
                })
            .WithName("GetTasks")
            .Produces<GetTasksResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Get All Tasks")
            .WithDescription("Retrieve tasks with optional filters (Employees see only their assigned tasks, Managers and Admins see all tasks)");
        }
    }
}
