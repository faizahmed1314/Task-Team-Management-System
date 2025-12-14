using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.GetUserById
{
    public record GetUserByIdResponse(User User);

    public class GetUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users/{id}",
                async (int id, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var query = new GetUserByIdQuery(id);

                            var result = await sender.Send(query);

                            var response = result.Adapt<GetUserByIdResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin, Role.Manager
                    );
                })
            .WithName("GetUserById")
            .Produces<GetUserByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get User By ID")
            .WithDescription("Retrieve a specific user by their ID (Admin and Manager Only)");
        }
    }
}