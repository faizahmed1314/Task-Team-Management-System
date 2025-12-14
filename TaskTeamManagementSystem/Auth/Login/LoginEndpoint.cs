using Carter;
using FluentValidation;
using MediatR;
using TaskTeamManagementSystem.Authentication;

namespace TaskTeamManagementSystem.Auth.Login;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string Email,
    string FullName,
    string Role,
    int UserId
);

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login",
            async (LoginRequest request, ISender sender) =>
            {
                var validator = new LoginRequestValidator();
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var command = new LoginCommand(request.Email, request.Password);
                var result = await sender.Send(command);

                if (result == null)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Unauthorized",
                        detail: "Invalid email or password"
                    );
                }

                var response = new LoginResponse(
                    result.Token,
                    result.Email,
                    result.FullName,
                    result.Role,
                    result.UserId
                );

                return Results.Ok(response);
            })
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("User Login")
        .WithDescription("Authenticate user and return JWT token")
        .AllowAnonymous();
    }
}
