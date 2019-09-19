using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using squittal.LivePlanetmans.Server.Services.Planetside;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Data
{
    public static class DbInitializer
    {
        //private static readonly IWorldService _worldService;
        
        public static void Initialize(IServiceProvider serviceProvider)//, IWorldService worldService)
        {
            
            using (var context = new PlanetmansDbContext(
                serviceProvider.GetRequiredService<
                DbContextOptions<PlanetmansDbContext>>()))
            {
                // Always run migrations before seeding data
                context.Database.Migrate();

                // Populate Faction & World tables here
                //var worldService = serviceProvider.GetRequiredService<WorldService>();
                //await worldService.RefreshStore();

                //Services.WorldService.RefreshStore();

                return;
            }
        }

        //public static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app)
        //{
        //    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        //    {
        //        var dbContext = serviceScope.ServiceProvider.GetRequiredService<PlanetmansDbContext>();
        //        dbContext.Database.Migrate();

        //        var worldService = serviceScope.ServiceProvider.GetRequiredService<WorldService>();
        //        await worldService.RefreshStore();
        //    }
        //    return app;
        //}
    }
}
