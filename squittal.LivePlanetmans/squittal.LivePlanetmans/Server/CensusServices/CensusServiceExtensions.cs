using Microsoft.Extensions.DependencyInjection;

namespace squittal.LivePlanetmans.Server.CensusServices
{
    public static class CensusServiceExtensions
    {
        public static IServiceCollection AddCensusHelpers(this IServiceCollection services)
        {
            services.AddSingleton<CensusCharacter>();
            services.AddSingleton<CensusFaction>();
            services.AddSingleton<CensusItem>();
            services.AddSingleton<CensusItemCategory>();
            services.AddSingleton<CensusOutfit>();
            services.AddSingleton<CensusWorld>();
            services.AddSingleton<CensusZone>();

            return services;
        }
    }
}
