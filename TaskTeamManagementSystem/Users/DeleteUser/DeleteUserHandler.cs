using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace TaskTeamManagementSystem.Users.DeleteUser
{
    public record DeleteUserCommand(int Id) : ICommand<DeleteUserResult>;
    public record DeleteUserResult(bool IsSuccess);

    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        public DeleteUserCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("User ID must be greater than 0.");
        }
    }

    public class DeleteUserCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<DeleteUserCommand, DeleteUserResult>
    {
        public async Task<DeleteUserResult> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

            if (user == null)
            {
                throw new Exception($"User with ID {command.Id} not found");
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteUserResult(true);
        }
    }
}