using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tasks.CreateTask
{
    public record CreateTaskRequest(TaskItem Task);
    public record CreateTaskResponse(int Id);

    public class CreateTaskEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/tasks",
                async (CreateTaskRequest request, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var command = request.Adapt<CreateTaskCommand>();

                            var result = await sender.Send(command);

                            var response = result.Adapt<CreateTaskResponse>();

                            return Results.Created($"/tasks/{response.Id}", response);
                        },
                        Role.Manager, Role.Admin
                    );
                })
            .WithName("CreateTask")
            .Produces<CreateTaskResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Create Task")
            .WithDescription("Create a new task (Manager and Admin Only)");
        }
    }
}
