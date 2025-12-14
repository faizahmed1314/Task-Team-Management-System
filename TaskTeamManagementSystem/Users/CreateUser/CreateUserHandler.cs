using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;

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
    
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResult>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(IApplicationDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<CreateUserResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            // Hash the password before storing
            var user = command.User;
            user.Password = _passwordHasher.HashPassword(user.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateUserResult(user.Id);
        }
    }
}
