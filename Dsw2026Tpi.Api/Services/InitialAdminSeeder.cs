using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Dsw2026Tpi.Api.Services;

public class InitialAdminSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InitialAdminSeeder> _logger;

    public InitialAdminSeeder(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<InitialAdminSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = _configuration["AdminSeed:Email"]
            ?? throw new InvalidOperationException("Falta AdminSeed:Email en la configuración");
        var adminPassword = _configuration["AdminSeed:Password"]
            ?? throw new InvalidOperationException("Falta AdminSeed:Password en la configuración");

        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
        var result = await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Administrator);
            _logger.LogInformation("Admin inicial creado: {Email}", adminEmail);
        }
        else
        {
            _logger.LogWarning("No se pudo crear el admin inicial: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
