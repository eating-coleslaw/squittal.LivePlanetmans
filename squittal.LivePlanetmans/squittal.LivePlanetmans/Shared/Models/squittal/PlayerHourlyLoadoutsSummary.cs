using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyLoadoutsSummary
    {
        public string PlayerId { get; set; }
        public int PlayerFactionId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public IEnumerable<HourlyLoadoutSummaryRow> LoadoutAggregates { get; set; }

        //public IEnumerable<PlayerHourlyLoadoutsSummaryRow1> asgasgd;
        public IEnumerable<PlayerHourlyLoadoutsSummaryRow> TopLoadoutsByKills { get; set; }
        public IEnumerable<PlayerHourlyLoadoutsSummaryRow> TopLoadoutsByDeaths { get; set; }

        //public IEnumerable<FactionLoadoutSummaryRow> PlayerLoadouts;
        //public IEnumerable<ProfileTypeSummaryRow> EnemyLoadouts;
        //public IEnumerable<FactionLoadoutSummaryRow> EnemyFactionLoadouts;
    }

    public class HourlyLoadoutSummaryRow
    {
        public int LoadoutId { get; set; }
        public string LoadoutName { get; set; }
        public int ProfileId { get; set; }
        public int FactionId { get; set; }
        public DeathEventAggregate LoadoutDeathEventAggregate { get; set; }
        public IEnumerable<VictimLoadoutSummary> LoadoutVsLoadoutAggregates { get; set; }

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

        public DeathEventAggregate AttackerDeathEventAggregate { get; set; }

        //public DeathEventAggregate VictimDeathEventAggregate { get; set; }
    }

    public class VictimLoadoutSummary
    {
        public int VictimLoadoutId { get; set; }
        public string VictimLoadoutName { get; set; }
        public int VictimProfileId { get; set; }
        public int VictimFactionId { get; set; }

        public DeathEventAggregate AttackerDeathEventAggregate { get; set; }
    }

    public class PlayerHourlyLoadoutsSummaryRow
    {
        public int AttackerLoadoutId { get; set; }
        public string AttackerLoadoutName { get; set; }
        public int? AttackerFactionId { get; set; }
        public int AttackerProfileId { get; set; }

        public int VictimLoadoutId { get; set; }
        public string VictimLoadoutName { get; set; }
        public int? VictimFactionId { get; set; }
        public int VictimProfileId { get; set; }

        public int AttackerKills { get; set; }
        public int AttackerHeadshots { get; set; }
        public int VictimKills { get; set; }
        public int VictimHeadshots { get; set; }

        public double AttackerKillDeathRatio
        {
            get
            {
                return GetKillDeathRatio(AttackerKills, VictimKills);
            }
        }

        public double AttackerHeadshotRatio
        {
            get
            {
                return GetHeadshotRatio(AttackerKills, AttackerHeadshots);
            }
        }

        public double VictimKillDeathRatio
        {
            get
            {
                return GetKillDeathRatio(VictimKills, AttackerKills);
            }
        }

        public double VictimHeadshotRatio
        {
            get
            {
                return GetHeadshotRatio(VictimKills, VictimHeadshots);
            }
        }

        private double GetKillDeathRatio(int kills, int deaths)
        {
            if (deaths == 0)
            {
                return (kills / 1.0);
            }
            else
            {
                return Math.Round((double)(kills / (double)deaths), 2);
            }
        }

        private double GetHeadshotRatio(int kills, int headshots)
        {
            if (kills > 0)
            {
                return Math.Round((double)headshots / (double)kills * 100.0, 1);
            }
            else
            {
                return 0;
            }
        }
    }

    /*
    public class FactionLoadoutSummaryRow //: LoadoutSummaryRow
    {
        public int FactionId { get; set; }
        public int LoadoutId { get; set; }
        public int Kills { get; set; }
        public int Headshots { get; set; }
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
    */


}
