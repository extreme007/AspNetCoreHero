using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCoreHero.PublicAPI
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var _logger = host.Services.GetService<ILogger<Program>>();
            using (var scope = host.Services.CreateScope())
            {
                _logger.LogInformation("Loading Application");
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await Infrastructure.Persistence.Seeds.IdentityContextSeed.SeedRolesAsync(userManager, roleManager);
                    await Infrastructure.Persistence.Seeds.IdentityContextSeed.SeedAdminAsync(userManager, roleManager);
                    await Infrastructure.Persistence.Seeds.IdentityContextSeed.SeedBasicUserAsync(userManager, roleManager);

                    _logger.LogInformation("Finished Seeding Default Data");
                    _logger.LogInformation("Application Starting");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "An error occurred seeding the DB");
                }              
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
