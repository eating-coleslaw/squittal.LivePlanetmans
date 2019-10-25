using System;
using System.Collections.Generic;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class HourlyPlayerWeaponsReport
    {
        public string PlayerId { get; set; }
        public int PlayerFactionId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public IEnumerable<WeaponSummaryRow> TopWeaponsByKills { get; set; }
        public IEnumerable<WeaponSummaryRow> TopWeaponsByDeaths { get; set; }
    }

    public class WeaponSummaryRow
    {
        public int WeaponId { get; set; }
        public string WeaponName { get; set; }
        public int FactionId { get; set; }

        public DeathEventAggregate WeaponStats { get; set; }
        public DeathEventAggregate WeaponStatsAsVictim { get; set; }
    }
}
