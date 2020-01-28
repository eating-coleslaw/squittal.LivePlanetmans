using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services
{
    public interface IApplicationMetaDataService
    {
        Task<ApplicationStart> AddNewStartup(DateTime startTimeUtc);
        Task<ApplicationStart> GetLastStart();
        Task<ApplicationStart> GetFirstStartInTimeSpan(TimeSpan timeSpan, DateTime endTimeUtc);
    }
}
