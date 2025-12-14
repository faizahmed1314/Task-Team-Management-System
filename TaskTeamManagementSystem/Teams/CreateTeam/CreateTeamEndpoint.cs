using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.CreateTeam
{
    public record CreateTeamRequest(Team Team);
    public record CreateTeamResponse(int Id);

    public class CreateTeamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/teams",
                async (CreateTeamRequest request, ISender sender) =>
                {
                    var command = request.Adapt<CreateTeamCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<CreateTeamResponse>();

                    return Results.Created($"/teams/{response.Id}", response);
                })
            .WithName("CreateTeam")
            .Produces<CreateTeamResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Team")
            .WithDescription("Create a new team");
        }
    }
}
