﻿using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.CensusServices;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public class ProfileService : IProfileService, IDisposable
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly CensusProfile _censusProfile;
        private readonly CensusLoadout _censusLoadout;

        private Dictionary<int, Profile> _loadoutMapping = new Dictionary<int, Profile>();

        private readonly SemaphoreSlim _loadoutSemaphore = new SemaphoreSlim(1);

        public ProfileService(IDbContextHelper dbContextHelper, CensusProfile censusProfile, CensusLoadout censusLoadout)
        {
            _dbContextHelper = dbContextHelper;
            _censusProfile = censusProfile;
            _censusLoadout = censusLoadout;
        }

        public async Task<IEnumerable<Profile>> GetAllProfilesAsync()
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Profiles.ToListAsync();
            }
        }

        public async Task<IEnumerable<Loadout>> GetAllLoadoutsAsync()
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Loadouts.ToListAsync();
            }
        }

        public async Task<Profile> GetProfileFromLoadoutIdAsync(int loadoutId)
        {
            if (_loadoutMapping == null || _loadoutMapping.Count == 0)
            {
                await SetupLoadoutMappingAsync();
            }

            return _loadoutMapping.GetValueOrDefault(loadoutId, null);
        }

        private async Task SetupLoadoutMappingAsync()
        {
            await _loadoutSemaphore.WaitAsync();

            try
            {
                if (_loadoutMapping != null || _loadoutMapping.Count > 0)
                {
                    return;
                }

                var loadoutsTask = GetAllLoadoutsAsync();
                var profilesTask = GetAllProfilesAsync();

                await Task.WhenAll(loadoutsTask, profilesTask);

                var loadouts = loadoutsTask.Result;
                var profiles = profilesTask.Result;

                _loadoutMapping = loadouts.ToDictionary(l => l.Id, l => profiles.FirstOrDefault(p => p.Id == l.ProfileId));
            }
            finally
            {
                _loadoutSemaphore.Release();
            }
        }


        public async Task RefreshStore()
        {
            var censusProfiles = await _censusProfile.GetAllProfilesAsync();

            if (censusProfiles != null)
            {
                await UpsertRangeAsync(censusProfiles.Select(ConvertToDbModel));
            }

            var censusLoadouts = await _censusLoadout.GetAllLoadoutsAsync();

            if (censusLoadouts != null)
            {
                var allLoadouts = new List<CensusLoadoutModel>();

                allLoadouts.AddRange(censusLoadouts.ToList());
                allLoadouts.AddRange(GetFakeNsCensusLoadoutModels());

                await UpsertRangeAsync(allLoadouts.AsEnumerable().Select(ConvertToDbModel));
            }

        }

        private async Task UpsertRangeAsync(IEnumerable<Profile> censusEntities)
        {
            var createdEntities = new List<Profile>();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storedEntities = await dbContext.Profiles.ToListAsync();

                foreach (var censusEntity in censusEntities)
                {
                    var storeEntity = storedEntities.FirstOrDefault(e => e.Id == censusEntity.Id);
                    if (storeEntity == null)
                    {
                        createdEntities.Add(censusEntity);
                    }
                    else
                    {
                        storeEntity = censusEntity;
                        dbContext.Profiles.Update(storeEntity);
                    }
                }

                if (createdEntities.Any())
                {
                    await dbContext.Profiles.AddRangeAsync(createdEntities);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        private async Task UpsertRangeAsync(IEnumerable<Loadout> censusEntities)
        {
            var createdEntities = new List<Loadout>();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storedEntities = await dbContext.Loadouts.ToListAsync();

                foreach (var censusEntity in censusEntities)
                {
                    var storeEntity = storedEntities.FirstOrDefault(e => e.Id == censusEntity.Id);
                    if (storeEntity == null)
                    {
                        createdEntities.Add(censusEntity);
                    }
                    else
                    {
                        storeEntity = censusEntity;
                        dbContext.Loadouts.Update(storeEntity);
                    }
                }

                if (createdEntities.Any())
                {
                    await dbContext.Loadouts.AddRangeAsync(createdEntities);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        private Loadout ConvertToDbModel(CensusLoadoutModel censusModel)
        {
            return new Loadout
            {
                Id = censusModel.LoadoutId,
                ProfileId = censusModel.ProfileId,
                FactionId = censusModel.FactionId,
                CodeName = censusModel.CodeName,
            };
        }

        private Profile ConvertToDbModel(CensusProfileModel censusModel)
        {
            return new Profile
            {
                Id = censusModel.ProfileId,
                ProfileTypeId = censusModel.ProfileTypeId,
                FactionId = censusModel.FactionId,
                Name = censusModel.Name.English,
                ImageId = censusModel.ImageId
            };
        }

        private IEnumerable<CensusLoadoutModel> GetFakeNsCensusLoadoutModels()
        {
            var nsLoadouts = new List<CensusLoadoutModel>
            {
                GetNewCensusLoadoutModel(28, 190, 4, "NS Infiltrator"),
                GetNewCensusLoadoutModel(29, 191, 4, "NS Light Assault"),
                GetNewCensusLoadoutModel(30, 192, 4, "NS Combat Medic"),
                GetNewCensusLoadoutModel(31, 193, 4, "NS Engineer"),
                GetNewCensusLoadoutModel(32, 194, 4, "NS Heavy Assault")
            };

            return nsLoadouts;
        }

        private CensusLoadoutModel GetNewCensusLoadoutModel(int loadoutId, int profileId, int factionId, string codeName)
        {
            return new CensusLoadoutModel()
            {
                LoadoutId = loadoutId,
                ProfileId = profileId,
                FactionId = factionId,
                CodeName = codeName
            };
        }

        public void Dispose()
        {
            _loadoutSemaphore.Dispose();
        }
    }
}
