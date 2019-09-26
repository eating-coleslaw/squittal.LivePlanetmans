using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.CensusServices;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public class TitleService : ITitleService
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly CensusTitle _censusTitle;

        public TitleService(IDbContextHelper dbContextHelper, CensusTitle censusTitle)
        {
            _dbContextHelper = dbContextHelper;
            _censusTitle = censusTitle;
        }

        public async Task<IEnumerable<Title>> GetAllTitlesAsync()
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Titles.ToListAsync();
            }
        }

        public async Task<Title> GetTitleAsync(int titleId)
        {
            var titles = await GetAllTitlesAsync();
            return titles.FirstOrDefault(t => t.Id == titleId);
        }

        public async Task RefreshStore()
        {
            var createdEntities = new List<Title>();

            var titles = await _censusTitle.GetAllTitles();

            if (titles != null)
            {
                var censusEntities = titles.Select(ConvertToDbModel);

                using (var factory = _dbContextHelper.GetFactory())
                {
                    var dbContext = factory.GetDbContext();

                    var storedEntities = await dbContext.Titles.ToListAsync();

                    if (storedEntities.Any() == true)
                    {
                        return;
                    }

                    foreach (var censusEntity in censusEntities)
                    {
                        var storeEntity = storedEntities.FirstOrDefault(storedEntity => storedEntity.Id == censusEntity.Id);
                        if (storeEntity == null)
                        {
                            createdEntities.Add(censusEntity);
                        }
                        else
                        {
                            storeEntity = censusEntity;
                            dbContext.Titles.Update(storeEntity);
                        }
                    }

                    if (createdEntities.Any())
                    {
                        await dbContext.Titles.AddRangeAsync(createdEntities);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public static Title ConvertToDbModel(CensusTitleModel censusModel)
        {
            return new Title
            {
                Id = censusModel.TitleId,
                Name = censusModel.Name.English
            };
        }
    }
}
