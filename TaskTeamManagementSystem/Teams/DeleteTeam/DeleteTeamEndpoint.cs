using Carter;
using Mapster;
using MediatR;

namespace TaskTeamManagementSystem.Teams.DeleteTeam
{
    public record DeleteTeamResponse(bool IsSuccess);

    public class DeleteTeamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/teams/{id}",
                async (int id, ISender sender) =>
                {
                    var command = new DeleteTeamCommand(id);

                    var result = await sender.Send(command);

                    var response = result.Adapt<DeleteTeamResponse>();

                    return Results.Ok(response);
                })
            .WithName("DeleteTeam")
            .Produces<DeleteTeamResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Team")
            .WithDescription("Delete a team from the database");
        }
    }
}
