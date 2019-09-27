using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DaybreakGames.Census;
using DaybreakGames.Census.JsonConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using squittal.LivePlanetmans.CensusStream.Models;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.CensusStream
{
    public class WebsocketEventHandler : IWebsocketEventHandler
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly ICharacterService _characterService;
        private readonly ILogger<WebsocketEventHandler> _logger;
        private readonly Dictionary<string, MethodInfo> _processMethods;

        // Credit to Voidwell @Lampjaw
        private readonly JsonSerializer _payloadDeserializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new UnderscorePropertyNamesContractResolver(),
            Converters = new JsonConverter[]
                {
                    new BooleanJsonConverter(),
                    new DateTimeJsonConverter()
                }
        });

        public WebsocketEventHandler(IDbContextHelper dbContextHelper, ICharacterService characterService, ILogger<WebsocketEventHandler> logger)
        {
            _dbContextHelper = dbContextHelper;
            _characterService = characterService;
            _logger = logger;

            // Credit to Voidwell @ Lampjaw
            _processMethods = GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<CensusEventHandlerAttribute>() != null)
                .ToDictionary(m => m.GetCustomAttribute<CensusEventHandlerAttribute>().EventName);
        }

        public async Task Process(JToken message)
        {
            await ProcessServiceEvent(message);
        }

        // Credit to Voidwell @Lampjaw
        private async Task ProcessServiceEvent(JToken message)
        {
            var jPayload = message.SelectToken("payload");

            var payload = jPayload?.ToObject<PayloadBase>(_payloadDeserializer);
            var eventName = payload?.EventName;

            if (eventName == null)
            {
                return;
            }

            _logger.LogDebug("Payload received for event: {0}.", eventName);

            if (!_processMethods.ContainsKey(eventName))
            {
                _logger.LogWarning("No process method found for event: {0}", eventName);
                return;
            }

            if (payload.ZoneId.HasValue && payload.ZoneId.Value > 1000)
            {
                return;
            }

            try
            {
                var inputType = _processMethods[eventName].GetCustomAttribute<CensusEventHandlerAttribute>().PayloadType;
                var inputParam = jPayload.ToObject(inputType, _payloadDeserializer);

                await (Task)_processMethods[eventName].Invoke(this, new[] { inputParam });
            }
            catch (Exception ex)
            {
                _logger.LogError(75642, ex, "Failed to process websocket event: {0}.", eventName);
            }
        }

        [CensusEventHandler("Death", typeof(DeathPayload))]
        private async Task Process(DeathPayload payload)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                try
                {
                    var TaskList = new List<Task>();
                    Task<OutfitMember> attackerOutfitTask = null;
                    Task<OutfitMember> victimOutfitTask = null;

                    //dbContext.Deaths.Add(dataModel);
                    //Task saveDeath = dbContext.SaveChangesAsync();
                    //TaskList.Add(saveDeath);

                    if (payload.AttackerCharacterId != null && payload.AttackerCharacterId.Length > 18)
                    {
                        attackerOutfitTask = _characterService.GetCharactersOutfitAsync(payload.AttackerCharacterId);
                        //Task<Character> attackerTask = _characterService.GetCharacterAsync(payload.AttackerCharacterId);
                        TaskList.Add(attackerOutfitTask);
                    }
                    if (payload.AttackerCharacterId != null && payload.AttackerCharacterId.Length > 18)
                    {
                        victimOutfitTask = _characterService.GetCharactersOutfitAsync(payload.CharacterId);
                        //Task<Character> victimTask = _characterService.GetCharacterAsync(payload.CharacterId);
                        TaskList.Add(victimOutfitTask);
                    }

                    await Task.WhenAll(TaskList);
                    TaskList.Clear();

                    int attackerFactionId = await dbContext.Characters
                                                        .Where(c => c.Id == payload.AttackerCharacterId)
                                                        .Select(c => c.FactionId)
                                                        .FirstOrDefaultAsync();

                    //Task<int> attackerFactionTask = dbContext.Characters
                    //                                    .AsNoTracking()
                    //                                    .Where(c => c.Id == payload.AttackerCharacterId)
                    //                                    .Select(c => c.FactionId)
                    //                                    .FirstOrDefaultAsync();

                    //TaskList.Add(attackerFactionTask);

                    int victimFactionId = await dbContext.Characters
                                                        .Where(c => c.Id == payload.CharacterId)
                                                        .Select(c => c.FactionId)
                                                        .FirstOrDefaultAsync();

                    //Task<int> victimFactionTask = dbContext.Characters
                    //                                    .AsNoTracking()
                    //                                    .Where(c => c.Id == payload.CharacterId)
                    //                                    .Select(c => c.FactionId)
                    //                                    .FirstOrDefaultAsync();
                    //TaskList.Add(victimFactionTask);

                    //await Task.WhenAll(TaskList);

                    var dataModel = new Shared.Models.Death
                    {
                        AttackerCharacterId = payload.AttackerCharacterId,
                        AttackerFireModeId = payload.AttackerFireModeId,
                        AttackerLoadoutId = payload.AttackerLoadoutId,
                        AttackerVehicleId = payload.AttackerVehicleId,
                        AttackerWeaponId = payload.AttackerWeaponId,
                        AttackerOutfitId = attackerOutfitTask?.Result?.OutfitId,
                        AttackerFactionId = attackerFactionId, //attackerFactionTask?.Result,
                        CharacterId = payload.CharacterId,
                        CharacterLoadoutId = payload.CharacterLoadoutId,
                        CharacterOutfitId = victimOutfitTask?.Result?.OutfitId,
                        CharacterFactionId = victimFactionId, //victimFactionTask?.Result,
                        IsHeadshot = payload.IsHeadshot,
                        Timestamp = payload.Timestamp,
                        WorldId = payload.WorldId,
                        ZoneId = payload.ZoneId.Value
                    };

                    dbContext.Deaths.Add(dataModel);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    //Ignore
                }
            }
        }

        public void Dispose()
        {
            return;
        }
    }
}
