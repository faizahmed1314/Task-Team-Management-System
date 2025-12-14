using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.GetUsers
{
    public record GetUsersResponse(List<User> Users);

    public class GetUsersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users",
                async (ISender sender) =>
                {
                    var query = new GetUsersQuery();

                    var result = await sender.Send(query);

                    var response = result.Adapt<GetUsersResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetUsers")
            .Produces<GetUsersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get All Users")
            .WithDescription("Retrieve all users from the database");
        }
    }
}