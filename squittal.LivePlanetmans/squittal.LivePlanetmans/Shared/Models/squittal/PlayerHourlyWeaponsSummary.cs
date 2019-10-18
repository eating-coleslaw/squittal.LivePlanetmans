using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyWeaponsSummary
    {
        public string PlayerId { get; set; }
        public int PlayerFactionId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public HourlyWeaponSummaryRow[] TopWeaponsByKills { get; set; }
        public HourlyWeaponSummaryRow[] TopWeaponsByDeaths { get; set; }

        //public IEnumerable<HourlyWeaponSummaryRow> TopWeaponsByKills { get; set; }
        //public IEnumerable<HourlyWeaponSummaryRow> TopWeaponsByDeaths { get; set; }

    }

    public class HourlyWeaponSummaryRow
    {
        public int WeaponId { get; set; }
        public string WeaponName { get; set; }
        public int? FactionId { get; set; }
        
        public int Kills { get; set; }
        public int Headshots { get; set; }
        public double HeadshotRatio
        {
            get
            {
                return GetHeadshotRatio(Kills, Headshots);
            }
        }
        
        //public int DeathsAgainst { get; set; }
        //public int HeadshotsAgainst { get; set; }
        //public double HeadshotRatioAgainst
        //{
        //    get
        //    {
        //        return GetHeadshotRatio(DeathsAgainst, HeadshotsAgainst);
        //    }
        //}


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
