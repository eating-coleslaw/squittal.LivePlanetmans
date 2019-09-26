using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyStatsData
    {
        //public string Faction { get; set; }
        public int FactionId { get; set; }
        public string FactionName { get; set; }
        public string OutfitAlias { get; set; }
        public string OutfitId { get; set; }
        public string OutfitName { get; set; }
        public string OutfitRankName { get; set; }
        public string PlayerName { get; set; }
        public string PlayerId { get; set; }
        public string TitleName { get; set; }
        public string WorldName { get; set; }
        public int BattleRank { get; set; }
        public int PrestigeLevel { get; set; }
        public int LatestZoneId { get; set; }
        public string LatestZoneName { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Headshots { get; set; }
        public int TeamKills { get; set; }
        public int Suicides { get; set; }
        public double KillDeathRatio
        {
            get
            {
                if (Deaths == 0 )
                {
                    return (Kills / 1.0);
                }
                else
                {
                    return Math.Round((double)(Kills / (double)Deaths), 2);
                }
            }
        }
        public double KillsPerMinute
        {
            get
            {
                return Math.Round((double)(Kills / 60.0), 2);
            }
        }

        public double HeadshotRatio
        {
            get
            {
                return Math.Round((double)((double)Headshots / (double)Kills) * 100.0, 1);
            }
        }
    }
}
