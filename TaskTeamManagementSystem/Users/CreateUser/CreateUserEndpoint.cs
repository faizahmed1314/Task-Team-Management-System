using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.CreateUser
{
    public record CreateUserRequest(User User);

    public record CreateUsereResponse(int Id);
    public class CreateUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/users",
                async (CreateUserRequest request, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var command = request.Adapt<CreateUserCommand>();

                            var result = await sender.Send(command);

                            var response = result.Adapt<CreateUsereResponse>();

                            return Results.Created($"/users/{response.Id}", response);
                        },
                        Role.Admin
                    );
                })
            .WithName("CreateUser")
            .Produces<CreateUsereResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Create User")
            .WithDescription("Create User (Admin Only)");
        }
    }
}
