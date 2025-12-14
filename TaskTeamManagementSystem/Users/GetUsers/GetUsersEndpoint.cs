using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.GetUsers
{
    public record GetUsersResponse(
        List<User> Users,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetUsersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users",
                async (ISender sender,
                       HttpContext context,
                       IAuthorizationService authService,
                       Role? role,
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
                            var query = new GetUsersQuery(
                                Role: role,
                                SortBy: sortBy,
                                SortDescending: sortDescending,
                                PageNumber: pageNumber,
                                PageSize: pageSize
                            );

                            var result = await sender.Send(query);

                            var response = result.Adapt<GetUsersResponse>();

                            return Results.Ok(response);
                        },
                        Role.Admin, Role.Manager
                    );
                })
            .WithName("GetUsers")
            .Produces<GetUsersResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Get All Users")
            .WithDescription("Retrieve users with optional filters (Admin and Manager Only)");
        }
    }
}