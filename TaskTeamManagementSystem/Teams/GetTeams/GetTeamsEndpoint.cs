using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.GetTeams
{
    public record GetTeamsResponse(
        List<Team> Teams,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetTeamsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/teams",
                async (ISender sender,
                       string? sortBy,
                       bool sortDescending = false,
                       int pageNumber = 1,
                       int pageSize = 10) =>
                {
                    var query = new GetTeamsQuery(
                        SortBy: sortBy,
                        SortDescending: sortDescending,
                        PageNumber: pageNumber,
                        PageSize: pageSize
                    );

                    var result = await sender.Send(query);

                    var response = result.Adapt<GetTeamsResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetTeams")
            .Produces<GetTeamsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get All Teams")
            .WithDescription("Retrieve teams with sorting (sortBy, sortDescending) and pagination (pageNumber, pageSize)");
        }
    }
}
