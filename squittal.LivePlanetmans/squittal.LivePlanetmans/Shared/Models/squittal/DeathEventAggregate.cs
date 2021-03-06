﻿using System;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class DeathEventAggregate
    {
        public int Kills { get; set; } = 0;
        public int Deaths { get; set; } = 0;
        public int Headshots { get; set; } = 0;
        public int HeadshotDeaths { get; set; } = 0;
        public int Suicides { get; set; } = 0;
        public int Teamkills { get; set; } = 0;
        public int TeamkillDeaths { get; set; } = 0;

        public double KillDeathRatio
        {
            get
            {
                if (Deaths == 0)
                {
                    return (Kills / 1.0);
                }
                else
                {
                    return Math.Round((double)(Kills / (double)Deaths), 2);
                }
            }
        }

        public double HeadshotRatio
        {
            get
            {
                return GetHeadshotRatio(Kills, Headshots);
            }
        }

        public double HeadshotDeathRatio
        {
            get
            {
                return GetHeadshotRatio(Deaths, HeadshotDeaths);
            }
        }

        public bool HasEvents
        {
            get
            {
                return (Kills > 0 || Deaths > 0 || Teamkills > 0);
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

        public DeathEventAggregate Add(DeathEventAggregate addend)
        {
            Kills += addend.Kills;
            Headshots += addend.Headshots;
            Teamkills += addend.Teamkills;
            Deaths += addend.Deaths;
            Suicides += addend.Suicides;
            HeadshotDeaths += addend.HeadshotDeaths;

            return this;
        }
    }
}
