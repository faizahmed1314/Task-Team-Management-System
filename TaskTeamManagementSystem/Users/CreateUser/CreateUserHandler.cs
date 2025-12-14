using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using TaskTeamManagementSystem.Models;

namespace TaskTeamManagementSystem.Users.CreateUser
{
    public record CreateUserCommand(User User) : ICommand<CreateUserResult>;
    public record CreateUserResult(int Id);

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.User.FullName).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.User.Email).NotEmpty().WithMessage("Email is required.");
            RuleFor(x => x.User.Password).NotEmpty().WithMessage("Password is required.");
            RuleFor(x => x.User.Role).IsInEnum().WithMessage("Role must be a valid value (Admin=0, Manager=1, Employee=2).");
        }
    }
    public class CreateUserCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<CreateUserCommand, CreateUserResult>
    {
        public async Task<CreateUserResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {

            //create Order entity from command object
            //save to database
            //return result 


            dbContext.Users.Add(command.User);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateUserResult(command.User.Id);

        }
    }
}
