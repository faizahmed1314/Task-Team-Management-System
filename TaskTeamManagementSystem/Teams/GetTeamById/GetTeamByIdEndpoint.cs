using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.GetTeamById
{
    public record GetTeamByIdResponse(Team Team);

    public class GetTeamByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/teams/{id}",
                async (int id, ISender sender) =>
                {
                    var query = new GetTeamByIdQuery(id);

                    var result = await sender.Send(query);

                    var response = result.Adapt<GetTeamByIdResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetTeamById")
            .Produces<GetTeamByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Team By ID")
            .WithDescription("Retrieve a specific team by its ID");
        }
    }
}
