using Carter;
using Mapster;
using MediatR;
using TaskTeamManagementSystem.Domain.Models;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tasks.GetTasks
{
    public record GetTasksResponse(
        List<TaskItem> Tasks,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetTasksEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/tasks",
                async (ISender sender, 
                       TaskStatus? status, 
                       int? assignedToUserId, 
                       int? teamId, 
                       DateTime? dueDateFrom, 
                       DateTime? dueDateTo,
                       string? sortBy,
                       bool sortDescending = false,
                       int pageNumber = 1,
                       int pageSize = 10) =>
                {
                    var query = new GetTasksQuery(
                        Status: status,
                        AssignedToUserId: assignedToUserId,
                        TeamId: teamId,
                        DueDateFrom: dueDateFrom,
                        DueDateTo: dueDateTo,
                        SortBy: sortBy,
                        SortDescending: sortDescending,
                        PageNumber: pageNumber,
                        PageSize: pageSize
                    );

                    var result = await sender.Send(query);

                    var response = result.Adapt<GetTasksResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetTasks")
            .Produces<GetTasksResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get All Tasks")
            .WithDescription("Retrieve tasks with optional filters (status, assignedToUserId, teamId, dueDateFrom, dueDateTo), sorting (sortBy, sortDescending), and pagination (pageNumber, pageSize)");
        }
    }
}
