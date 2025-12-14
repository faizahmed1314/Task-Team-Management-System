using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
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
                       HttpContext context,
                       IAuthorizationService authService,
                       string? sortBy,
                       bool sortDescending = false,
                       int pageNumber = 1,
                       int pageSize = 10) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
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
                        },
                        Role.Admin, Role.Manager
                    );
                })
            .WithName("GetTeams")
            .Produces<GetTeamsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Get All Teams")
            .WithDescription("Retrieve teams with sorting and pagination (Admin and Manager Only)");
        }
    }
}
