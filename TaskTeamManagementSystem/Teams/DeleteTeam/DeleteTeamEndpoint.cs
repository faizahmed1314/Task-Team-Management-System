using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.DeleteTeam
{
    public record DeleteTeamResponse(bool IsSuccess);

    public class DeleteTeamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/teams/{id}",
                async (int id, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var command = new DeleteTeamCommand(id);

                            var result = await sender.Send(command);

                            var response = result.Adapt<DeleteTeamResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin
                    );
                })
            .WithName("DeleteTeam")
            .Produces<DeleteTeamResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Team")
            .WithDescription("Delete a team from the database (Admin Only)");
        }
    }
}
