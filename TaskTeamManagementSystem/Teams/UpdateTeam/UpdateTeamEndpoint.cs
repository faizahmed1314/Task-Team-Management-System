using Carter;
using Mapster;
using MediatR;

namespace TaskTeamManagementSystem.Teams.UpdateTeam
{
    public record UpdateTeamRequest(string Name, string Description);
    public record UpdateTeamResponse(bool IsSuccess);

    public class UpdateTeamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/teams/{id}",
                async (int id, UpdateTeamRequest request, ISender sender) =>
                {
                    var command = new UpdateTeamCommand(
                        Id: id,
                        Name: request.Name,
                        Description: request.Description
                    );

                    var result = await sender.Send(command);

                    var response = result.Adapt<UpdateTeamResponse>();

                    return Results.Ok(response);
                })
            .WithName("UpdateTeam")
            .Produces<UpdateTeamResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Team")
            .WithDescription("Update an existing team's information");
        }
    }
}
