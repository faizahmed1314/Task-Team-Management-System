using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.UpdateUser
{
    public record UpdateUserRequest(string FullName, string Email, string Password, Role Role);
    public record UpdateUserResponse(bool IsSuccess);

    public class UpdateUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/users/{id}",
                async (int id, UpdateUserRequest request, ISender sender, HttpContext context, IAuthorizationService authService) =>
                {
                    return await AuthorizationFilter.AuthorizeAsync(
                        context,
                        authService,
                        async (user) =>
                        {
                            var command = new UpdateUserCommand(
                                Id: id,
                                FullName: request.FullName,
                                Email: request.Email,
                                Password: request.Password,
                                Role: request.Role
                            );

                            var result = await sender.Send(command);

                            var response = result.Adapt<UpdateUserResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin
                    );
                })
            .WithName("UpdateUser")
            .Produces<UpdateUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update User")
            .WithDescription("Update an existing user's information (Admin Only)");
        }
    }
}