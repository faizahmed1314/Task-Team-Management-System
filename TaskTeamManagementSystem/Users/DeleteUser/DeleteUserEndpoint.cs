using Carter;
using Mapster;
using MediatR;

namespace TaskTeamManagementSystem.Users.DeleteUser
{
    public record DeleteUserResponse(bool IsSuccess);

    public class DeleteUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/users/{id}",
                async (int id, ISender sender) =>
                {
                    var command = new DeleteUserCommand(id);

                    var result = await sender.Send(command);

                    var response = result.Adapt<DeleteUserResponse>();

                    return Results.Ok(response);
                })
            .WithName("DeleteUser")
            .Produces<DeleteUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete User")
            .WithDescription("Delete a user from the database");
        }
    }
}