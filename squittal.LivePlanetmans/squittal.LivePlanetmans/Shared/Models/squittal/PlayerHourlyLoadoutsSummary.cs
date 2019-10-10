using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyLoadoutsSummary
    {
        public string PlayerId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public IEnumerable<FactionLoadoutSummaryRow> PlayerLoadouts;
        public IEnumerable<ProfileTypeSummaryRow> EnemyLoadouts;
        public IEnumerable<FactionLoadoutSummaryRow> EnemyFactionLoadouts;
    }

    public class FactionLoadoutSummaryRow : LoadoutSummaryRow
    {
        public int FactionId { get; set; }
        public int LoadoutId { get; set; }
    }

    public class ProfileTypeSummaryRow : LoadoutSummaryRow
    {
        //public int ProfileTypeId { get; set; }
    }

    public class LoadoutSummaryRow
    {
        //public int LoadoutId { get; set; }
        //public int FactionId { get; set; }
        public int ProfileTypeId { get; set; }
        public string Name { get; set; }
        public DeathEventAggregate Aggregates { get; set; }
    }

    
}
