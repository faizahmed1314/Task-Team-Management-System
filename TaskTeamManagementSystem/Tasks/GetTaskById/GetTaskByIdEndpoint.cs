using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tasks.GetTaskById
{
    public record GetTaskByIdResponse(TaskItem Task);

    public class GetTaskByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/tasks/{id}",
                async (int id, ISender sender) =>
                {
                    var query = new GetTaskByIdQuery(id);

                    var result = await sender.Send(query);

                    var response = result.Adapt<GetTaskByIdResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetTaskById")
            .Produces<GetTaskByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Task By ID")
            .WithDescription("Retrieve a specific task by its ID");
        }
    }
}
