using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestSite.Services.Identity;

namespace TestSite;

public class BackgroundTestService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BackgroundTestService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<TestSiteUser>>();

            var user = (await manager.FindByEmailAsync("test@test.ts"))!;
            var oldUsername = user.UserName;

            user.UserName = "User_" + new Random().Next();

            var result = await manager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"{error.Code} - {error.Description}");
                }
            }

            var newUser = (await manager.FindByEmailAsync("test@test.ts"))!;

            Console.WriteLine($"From {oldUsername} to {user.UserName} updated {newUser.UserName}, {user.UserName == newUser.UserName}");

            await Task.Delay(5 * 1000, stoppingToken);
        }
    }
}
