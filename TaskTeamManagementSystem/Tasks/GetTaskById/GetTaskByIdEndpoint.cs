using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tasks.GetTaskById
{
    public record GetTaskByIdResponse(TaskItem Task);

    public class GetTaskByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/tasks/{id}",
                async (int id, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var query = new GetTaskByIdQuery(id);

                            var result = await sender.Send(query);

                            var response = result.Adapt<GetTaskByIdResponse>();

                            if (user.Role == Role.Employee && response.Task?.AssignedToUserId != user.Id)
                            {
                                return Results.Problem(
                                    statusCode: StatusCodes.Status403Forbidden,
                                    title: "Forbidden",
                                    detail: "Employees can only view their assigned tasks"
                                );
                            }

                            return Results.Ok(response);
                        },
                        Role.Admin, Role.Manager, Role.Employee
                    );
                })
            .WithName("GetTaskById")
            .Produces<GetTaskByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Task By ID")
            .WithDescription("Retrieve a specific task by its ID (Employees can only view their assigned tasks)");
        }
    }
}
