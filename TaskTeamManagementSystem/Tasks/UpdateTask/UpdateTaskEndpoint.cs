using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tasks.UpdateTask
{
    public record UpdateTaskRequest(string Title, string Description, TaskStatus Status, 
        int AssignedToUserId, int CreatedByUserId, int TeamId, DateTime DueDate);
    public record UpdateTaskResponse(bool IsSuccess);

    public class UpdateTaskEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/tasks/{id}",
                async (int id, UpdateTaskRequest request, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeTaskUpdateAsync(
                        context,
                        authService,
                        id,
                        async (user) =>
                        {
                            var command = new UpdateTaskCommand(
                                Id: id,
                                Title: request.Title,
                                Description: request.Description,
                                Status: request.Status,
                                AssignedToUserId: request.AssignedToUserId,
                                CreatedByUserId: request.CreatedByUserId,
                                TeamId: request.TeamId,
                                DueDate: request.DueDate
                            );

                            var result = await sender.Send(command);

                            var response = result.Adapt<UpdateTaskResponse>();

                            return Results.Ok(response);
                        }
                    );
                })
            .WithName("UpdateTask")
            .Produces<UpdateTaskResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Task")
            .WithDescription("Update an existing task's information (Managers and Admins can update any task, Employees can only update their assigned tasks)");
        }
    }
}
