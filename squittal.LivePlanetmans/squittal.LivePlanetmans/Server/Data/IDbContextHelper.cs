using static squittal.LivePlanetmans.Server.Data.DbContextHelper;

namespace squittal.LivePlanetmans.Server.Data
{
    public interface IDbContextHelper
    {
        DbContextFactory GetFactory();
    }
}
