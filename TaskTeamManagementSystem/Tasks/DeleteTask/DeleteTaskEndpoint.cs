using Carter;
using Mapster;
using MediatR;

namespace TaskTeamManagementSystem.Tasks.DeleteTask
{
    public record DeleteTaskResponse(bool IsSuccess);

    public class DeleteTaskEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/tasks/{id}",
                async (int id, ISender sender) =>
                {
                    var command = new DeleteTaskCommand(id);

                    var result = await sender.Send(command);

                    var response = result.Adapt<DeleteTaskResponse>();

                    return Results.Ok(response);
                })
            .WithName("DeleteTask")
            .Produces<DeleteTaskResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Task")
            .WithDescription("Delete a task from the database");
        }
    }
}
