using Application.Data;
using MediatR;

namespace TaskTeamManagementSystem.Tasks.UpdateTaskStatus
{
    public class UpdateTaskStatusHandler : IRequestHandler<UpdateTaskStatusCommand, UpdateTaskStatusResult>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTaskStatusHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateTaskStatusResult> Handle(UpdateTaskStatusCommand command, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks.FindAsync(command.Id);

            if (task == null)
            {
                return new UpdateTaskStatusResult(false);
            }

            task.Status = command.Status;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateTaskStatusResult(true);
        }
    }
}
