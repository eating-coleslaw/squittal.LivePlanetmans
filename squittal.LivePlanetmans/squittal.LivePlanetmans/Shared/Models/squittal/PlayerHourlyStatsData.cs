using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyStatsData
    {
        //public string Faction { get; set; }
        public int FactionId { get; set; }
        //public string OutfitTag { get; set; }
        //public string OutfitId { get; set; }
        public string PlayerName { get; set; }
        public string PlayerId { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public double KillDeathRatio
        {
            get
            {
                if (Deaths < 1 )
                {
                    return (double)(Kills / 1.0);
                }
                else
                {
                    return Math.Round((double)((float)Kills / (float)Deaths), 2);
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
    }
}
