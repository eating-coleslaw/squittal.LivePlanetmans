using System;
using System.Collections.Generic;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerLoadoutsReport
    {
        public string PlayerId { get; set; }
        public int PlayerFactionId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public IEnumerable<FactionLoadoutsSummary> FactionLoadouts { get; set; }
        public IEnumerable<LoadoutSummary> PlayerLoadouts { get; set; }
        public DeathEventAggregate PlayerStats { get; set; }
    }

    public class FactionLoadoutsSummary
    {
        public FactionSummary Summary { get; set; }
        public IEnumerable<LoadoutSummary> PlayerLoadouts { get; set; }
        public IEnumerable<EnemyLoadoutHeadToHeadSummary> FactionLoadouts { get; set; }
    }

    public class EnemyLoadoutHeadToHeadSummary
    {
        public LoadoutSummary Summary { get; set; }
        public IEnumerable<LoadoutSummary> PlayerLoadouts { get; set; }
    }

    public class LoadoutSummary
    {
        public LoadoutDetails Details { get; set; }
        public DeathEventAggregate Stats { get; set; }
    }

    public class LoadoutDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public int FactionId { get; set; }
    }

    public class FactionSummary
    {
        public FactionDetails Details { get; set; }
        public DeathEventAggregate Stats { get; set; }
    }

    public class FactionDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ActiveFactionLoadouts
    {
        public int FactionId { get; set; }
        public IEnumerable<int> Loadouts { get; set; }
    }

    public class LoadoutVsLoadoutSummaryRow
    {
        public int AttackerLoadoutId { get; set; }
        public string AttackerLoadoutName { get; set; }
        public int AttackerProfileId { get; set; }
        public int AttackerFactionId { get; set; }

        public int VictimLoadoutId { get; set; }
        public string VictimLoadoutName { get; set; }
        public int VictimProfileId { get; set; }
        public int VictimFactionId { get; set; }

        public DeathEventAggregate AttackerStats { get; set; }
    }
}
