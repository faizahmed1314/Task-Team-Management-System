using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Models;

namespace TaskTeamManagementSystem.Users.CreateUser
{
    public record CreateUserRequest(User User);

    public record CreateUsereResponse(int Id);
    public class CreateUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/users",
                async (CreateUserRequest request, ISender sender) =>
                {
                    var command = request.Adapt<CreateUserCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<CreateUsereResponse>();

                    return Results.Created($"/users/{response.Id}", response);

                })
            .WithName("CreateUser")
            .Produces<CreateUsereResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create User")
            .WithDescription("Create User");
        }
    }
}
