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
    public class ItemService : IItemService
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly CensusItemCategory _censusItemCategory;
        private readonly CensusItem _censusItem;

        public ItemService(IDbContextHelper dbContextHelper, CensusItemCategory censusItemCategory, CensusItem censusItem)
        {
            _dbContextHelper = dbContextHelper;
            _censusItemCategory = censusItemCategory;
            _censusItem = censusItem;
        }

        public async Task<Item> GetItem(int itemId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Items.FirstOrDefaultAsync(i => i.Id == itemId);
            }
        }

        public async Task<IEnumerable<Item>> GetItemsByCategoryId(int categoryId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Items
                                .Where(i => i.ItemCategoryId == categoryId && i.ItemCategoryId.HasValue)
                                .ToListAsync();
            }
        }

        public async Task RefreshStore()
        {
            bool refreshStore = true;
            bool anyItems = false;
            bool anyCategories = false;

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                anyItems = await dbContext.Items.AnyAsync();

                if (anyItems == true)
                {
                    anyCategories = await dbContext.ItemCategories.AnyAsync();
                }

                refreshStore = (anyItems == false || anyCategories == false);
            }
            
            if (refreshStore != true)
            {
                return;
            }
            
            var itemCategories = await _censusItemCategory.GetAllItemCategories();

            if (itemCategories != null)
            {
                await UpsertRangeAsync(itemCategories.Select(ConvertToDbModel));
            }

            var items = await _censusItem.GetAllItems();

            if (items != null)
            {
                await UpsertRangeAsync(items.Select(ConvertToDbModel));
            }

        }


        private async Task UpsertRangeAsync(IEnumerable<Item> censusEntities)
        {
            var createdEntities = new List<Item>();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storedEntities = await dbContext.Items.ToListAsync();

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
                        dbContext.Items.Update(storeEntity);
                    }
                }

                if (createdEntities.Any())
                {
                    await dbContext.Items.AddRangeAsync(createdEntities);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        private async Task UpsertRangeAsync(IEnumerable<ItemCategory> censusEntities)
        {
            var createdEntities = new List<ItemCategory>();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storedEntities = await dbContext.ItemCategories.ToListAsync();

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
                        dbContext.ItemCategories.Update(storeEntity);
                    }
                }

                if (createdEntities.Any())
                {
                    await dbContext.ItemCategories.AddRangeAsync(createdEntities);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        private static Item ConvertToDbModel(CensusItemModel item)
        {
            return new Item
            {
                Id = item.ItemId,
                ItemTypeId = item.ItemTypeId,
                ItemCategoryId = item.ItemCategoryId,
                IsVehicleWeapon = item.IsVehicleWeapon,
                Name = item.Name?.English,
                Description = item.Description?.English,
                FactionId = item.FactionId,
                MaxStackSize = item.MaxStackSize,
                ImageId = item.ImageId
            };
        }

        private static ItemCategory ConvertToDbModel(CensusItemCategoryModel itemCategory)
        {
            return new ItemCategory
            {
                Id = itemCategory.ItemCategoryId,
                Name = itemCategory.Name.English
            };
        }
        
    }
}
