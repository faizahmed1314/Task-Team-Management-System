using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.UpdateUser
{
    public record UpdateUserCommand(int Id, string FullName, string Email, string Password, Role Role) 
        : ICommand<UpdateUserResult>;
    public record UpdateUserResult(bool IsSuccess);

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("User ID must be greater than 0.");
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(x => x.Role).IsInEnum().WithMessage("Role must be a valid value (Admin=0, Manager=1, Employee=2).");
        }
    }

    public class UpdateUserCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<UpdateUserCommand, UpdateUserResult>
    {
        public async Task<UpdateUserResult> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

            if (user == null)
            {
                throw new Exception($"User with ID {command.Id} not found");
            }

            user.FullName = command.FullName;
            user.Email = command.Email;
            user.Password = command.Password;
            user.Role = command.Role;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateUserResult(true);
        }
    }
}