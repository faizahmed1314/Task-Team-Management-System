using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskTeamManagementSystem.Infrastructure.Data;

namespace Ordering.Infrastructure.Data.Extensions;
public static class DatabaseExtentions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();

        await SeedAsync(context);
    }
    
    private static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedUserAsync(context);
        await SeedTeamAsync(context);
        await SeedTaskAsync(context);
    }

    private static async Task SeedUserAsync(ApplicationDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            await context.Users.AddRangeAsync(InitialData.Users);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedTeamAsync(ApplicationDbContext context)
    {
        if (!await context.Teams.AnyAsync())
        {
            await context.Teams.AddRangeAsync(InitialData.Teams);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedTaskAsync(ApplicationDbContext context)
    {
        if (!await context.Tasks.AnyAsync())
        {
            await context.Tasks.AddRangeAsync(InitialData.Tasks);
            await context.SaveChangesAsync();
        }
    }
}
