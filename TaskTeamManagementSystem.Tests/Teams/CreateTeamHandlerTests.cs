using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Teams.CreateTeam;

namespace TaskTeamManagementSystem.Tests.Teams;

public class CreateTeamHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CreateTeamCommandHandler _handler;

    public CreateTeamHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _handler = new CreateTeamCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldCreateTeam()
    {
        // Arrange
        var team = new Team
        {
            Name = "Development Team",
            Description = "Software development team"
        };

        var command = new CreateTeamCommand(team);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var createdTeam = await _dbContext.Teams.FindAsync(result.Id);
        createdTeam.Should().NotBeNull();
        createdTeam!.Name.Should().Be("Development Team");
        createdTeam.Description.Should().Be("Software development team");
    }

    [Fact]
    public async Task Handle_WithEmptyDescription_ShouldCreateTeam()
    {
        // Arrange
        var team = new Team
        {
            Name = "QA Team",
            Description = string.Empty
        };

        var command = new CreateTeamCommand(team);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdTeam = await _dbContext.Teams.FindAsync(result.Id);
        createdTeam.Should().NotBeNull();
        createdTeam!.Name.Should().Be("QA Team");
        createdTeam.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCreateMultipleTeams()
    {
        // Arrange
        var teams = new[]
        {
            new Team { Name = "Team 1", Description = "Description 1" },
            new Team { Name = "Team 2", Description = "Description 2" },
            new Team { Name = "Team 3", Description = "Description 3" }
        };

        // Act
        var results = new List<CreateTeamResult>();
        foreach (var team in teams)
        {
            var command = new CreateTeamCommand(team);
            var result = await _handler.Handle(command, CancellationToken.None);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Select(r => r.Id).Should().OnlyHaveUniqueItems();

        var allTeams = await _dbContext.Teams.ToListAsync();
        allTeams.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("DevOps Team")]
    [InlineData("Marketing")]
    [InlineData("Human Resources")]
    public async Task Handle_WithDifferentTeamNames_ShouldCreateCorrectly(string teamName)
    {
        // Arrange
        var team = new Team
        {
            Name = teamName,
            Description = $"Description for {teamName}"
        };

        var command = new CreateTeamCommand(team);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdTeam = await _dbContext.Teams.FindAsync(result.Id);
        createdTeam.Should().NotBeNull();
        createdTeam!.Name.Should().Be(teamName);
    }
}
