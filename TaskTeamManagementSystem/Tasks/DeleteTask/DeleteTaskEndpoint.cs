using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tasks.DeleteTask
{
    public record DeleteTaskResponse(bool IsSuccess);

    public class DeleteTaskEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/tasks/{id}",
                async (int id, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var command = new DeleteTaskCommand(id);

                            var result = await sender.Send(command);

                            var response = result.Adapt<DeleteTaskResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin, Role.Manager
                    );
                })
            .WithName("DeleteTask")
            .Produces<DeleteTaskResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Task")
            .WithDescription("Delete a task from the database (Admin and Manager Only)");
        }
    }
}
