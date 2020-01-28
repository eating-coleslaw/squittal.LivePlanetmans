using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using squittal.LivePlanetmans.Server.CensusServices;
using squittal.LivePlanetmans.Server.CensusStream;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Linq;

namespace squittal.LivePlanetmans.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();

            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });

            services.AddDbContext<PlanetmansDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("PlanetmansDbContext")));

            services.AddCensusServices(options =>
                options.CensusServiceId = Environment.GetEnvironmentVariable("DaybreakGamesServiceKey", EnvironmentVariableTarget.User));
            services.AddCensusHelpers();

            //services.AddTransient<IUpdateable, WorldService>();
            services.AddSingleton<IDbContextHelper, DbContextHelper>();
            services.AddSingleton<PlayerLoginMemoryCache>();

            services.AddTransient<IFactionService, FactionService>();
            services.AddTransient<IItemService, ItemService>();
            services.AddTransient<IZoneService, ZoneService>();
            services.AddTransient<ITitleService, TitleService>();

            services.AddSingleton<IWorldService, WorldService>();
            services.AddSingleton<ICharacterService, CharacterService>();
            services.AddSingleton<IOutfitService, OutfitService>();
            services.AddSingleton<IProfileService, ProfileService>();

            services.AddSingleton<IDbSeeder, DbSeeder>();
            services.AddSingleton<IApplicationMetaDataService, ApplicationMetaDataService>();

            services.AddSingleton<IWebsocketEventHandler, WebsocketEventHandler>();
            services.AddSingleton<IWebsocketMonitor, WebsocketMonitor>();

            services.AddHostedService<WebsocketMonitorHostedService>();
            services.AddHostedService<DbSeederHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Startup>();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Startup>("index.html");
            });
        }
    }
}
