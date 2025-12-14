using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.UpdateTeam
{
    public record UpdateTeamRequest(string Name, string Description);
    public record UpdateTeamResponse(bool IsSuccess);

    public class UpdateTeamEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/teams/{id}",
                async (int id, UpdateTeamRequest request, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var command = new UpdateTeamCommand(
                                Id: id,
                                Name: request.Name,
                                Description: request.Description
                            );

                            var result = await sender.Send(command);

                            var response = result.Adapt<UpdateTeamResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin
                    );
                })
            .WithName("UpdateTeam")
            .Produces<UpdateTeamResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Team")
            .WithDescription("Update an existing team's information (Admin Only)");
        }
    }
}
