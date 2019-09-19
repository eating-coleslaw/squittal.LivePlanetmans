using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services
{
    public interface IUpdateable
    {
        //string ServiceName { get; }
        //TimeSpan UpdateInterval { get; }
        Task RefreshStore();
    }
}
