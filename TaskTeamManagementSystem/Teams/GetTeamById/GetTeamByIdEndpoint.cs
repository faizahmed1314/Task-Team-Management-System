using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.GetTeamById
{
    public record GetTeamByIdResponse(Team Team);

    public class GetTeamByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/teams/{id}",
                async (int id, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var query = new GetTeamByIdQuery(id);

                            var result = await sender.Send(query);

                            var response = result.Adapt<GetTeamByIdResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin, Role.Manager
                    );
                })
            .WithName("GetTeamById")
            .Produces<GetTeamByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Team By ID")
            .WithDescription("Retrieve a specific team by its ID (Admin and Manager Only)");
        }
    }
}
