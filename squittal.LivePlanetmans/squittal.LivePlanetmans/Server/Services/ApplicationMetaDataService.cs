using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services
{
    public class ApplicationMetaDataService : IApplicationMetaDataService
    {
        private readonly IDbContextHelper _dbContextHelper;

        public ApplicationMetaDataService(IDbContextHelper dbContextHelper)
        {
            _dbContextHelper = dbContextHelper;
        }

        public async Task<ApplicationStart> AddNewStartup(DateTime startTimeUtc)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeEntity = await dbContext.ApplicationStarts
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(s => s.StartTimeUtc == startTimeUtc);

                if (storeEntity != null)
                {
                    return storeEntity;
                }

                var newEntity = new ApplicationStart
                {
                    StartTimeUtc = startTimeUtc
                };

                await dbContext.AddAsync(newEntity);
                await dbContext.SaveChangesAsync();

                return newEntity;
            }
        }

        public async Task<ApplicationStart> GetLastStart()
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeEntity = await dbContext.ApplicationStarts
                                            .AsNoTracking()
                                            .OrderByDescending(start => start.StartTimeUtc)
                                            .FirstOrDefaultAsync();

                return storeEntity;
            }
        }

        public async Task<ApplicationStart> GetFirstStartInTimeSpan(TimeSpan timeSpan, DateTime endTimeUtc)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var spanStart = endTimeUtc - timeSpan;

                var storeEntity = await dbContext.ApplicationStarts
                                            .AsNoTracking()
                                            .Where(start => start.StartTimeUtc >= spanStart)
                                            .OrderBy(start => start.StartTimeUtc)
                                            .FirstOrDefaultAsync();

                return storeEntity;
            }
        }
    }
}
