using System.Text.Json;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Api.Services;

public class PatientSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PatientSeeder> _logger;

    private record PatientSeed(string Name, string Email, string Dni);

    public PatientSeeder(IServiceProvider serviceProvider, ILogger<PatientSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<Dsw2026TpiDbContext>();

        var path = Path.Combine(AppContext.BaseDirectory, "Sources", "patients.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("No se encontró Sources/patients.json, se omite la carga inicial de pacientes");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var seeds = JsonSerializer.Deserialize<List<PatientSeed>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        foreach (var seed in seeds)
        {
            var alreadyExists = await context.Set<Patient>().AnyAsync(p => p.Dni == seed.Dni, cancellationToken);
            if (alreadyExists) continue;

            var user = await userManager.FindByEmailAsync(seed.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = seed.Email,
                    Email = seed.Email,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var password = $"{seed.Email}A{seed.Dni}"; //igual que login de patient
                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("No se pudo crear el usuario para {Email}: {Errors}",
                        seed.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(user, Roles.Patient);
            }

            context.Add(new Patient(seed.Name, seed.Dni, Guid.Parse(user.Id)));
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Carga inicial de pacientes completada");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}