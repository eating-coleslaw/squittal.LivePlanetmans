using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new PlanetmansDbContext(
                serviceProvider.GetRequiredService<
                DbContextOptions<PlanetmansDbContext>>()))
            {
                // Always run migrations before seeding data
                context.Database.Migrate();

                // Populate Faction & World tables here

                //Services.WorldService.RefreshStore();

                return;
            }
        }
    }
}
