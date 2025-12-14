using Application.Data;
using BuildingBlocks.Behavior;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ordering.Infrastructure.Data.Extensions;
using System.Text;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var assembly = typeof(Program).Assembly;

// Configure JWT Settings
var jwtSettings = new JwtSettings
{
    SecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
    Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "TaskTeamManagementSystem",
    Audience = builder.Configuration["JwtSettings:Audience"] ?? "TaskTeamManagementSystemUsers",
    ExpiryInMinutes = int.Parse(builder.Configuration["JwtSettings:ExpiryInMinutes"] ?? "60")
};

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidatorBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(assembly);

// Add Carter
builder.Services.AddCarter();

// Add Authorization Service
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Database");

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map Carter endpoints
app.MapCarter();

app.MapControllers();

app.Run();
