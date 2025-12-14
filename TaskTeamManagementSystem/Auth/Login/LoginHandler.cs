using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;

namespace TaskTeamManagementSystem.Auth.Login;

public record LoginCommand(string Email, string Password) : ICommand<LoginResult?>;

public record LoginResult(
    string Token,
    string Email,
    string FullName,
    string Role,
    int UserId
);

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResult?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult?> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user == null)
        {
            return null;
        }

        if (!_passwordHasher.VerifyPassword(command.Password, user.Password))
        {
            return null;
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResult(
            token,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.Id
        );
    }
}
