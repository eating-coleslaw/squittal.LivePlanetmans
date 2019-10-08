using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyHeadToHeadSummary
    {
        public string PlayerId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public IEnumerable<PlayerHourlyHeadToHeadSummaryRow> TopPlayersByKills { get; set; }
        public IEnumerable<PlayerHourlyHeadToHeadSummaryRow> TopPlayersByDeaths { get; set; }
    }

    public class PlayerHourlyHeadToHeadSummaryRow
    {
        public string AttackerCharacterId { get; set; }
        public string AttackerName { get; set; }
        public int? AttackerFactionId { get; set; }
        public string VictimCharacterId { get; set; }
        public string VictimName { get; set; }
        public int? VictimFactionId { get; set; }
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
}
