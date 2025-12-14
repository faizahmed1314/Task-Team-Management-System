using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tasks.UpdateTaskStatus
{
    public record UpdateTaskStatusRequest(TaskStatus Status);
    public record UpdateTaskStatusResponse(bool IsSuccess);

    public class UpdateTaskStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/tasks/{id}/status",
                async (int id, UpdateTaskStatusRequest request, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeTaskUpdateAsync(
                        context,
                        authService,
                        id,
                        async (user) =>
                        {
                            var command = new UpdateTaskStatusCommand(
                                Id: id,
                                Status: request.Status
                            );

                            var result = await sender.Send(command);

                            var response = result.Adapt<UpdateTaskStatusResponse>();

                            if (!response.IsSuccess)
                            {
                                return Results.NotFound(new { message = "Task not found" });
                            }

                            return Results.Ok(response);
                        }
                    );
                })
            .WithName("UpdateTaskStatus")
            .Produces<UpdateTaskStatusResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Task Status")
            .WithDescription("Update the status of a task (Employees can update their assigned tasks, Managers and Admins can update any task)");
        }
    }
}
