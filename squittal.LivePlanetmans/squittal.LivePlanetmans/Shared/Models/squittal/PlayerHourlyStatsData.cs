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
        public int WorldId { get; set; }
        public string WorldName { get; set; }
        public int BattleRank { get; set; }
        public int PrestigeLevel { get; set; }
        public int LatestZoneId { get; set; }
        public string LatestZoneName { get; set; }
        public DateTime? LatestLoginTime { get; set; }
        public DateTime? LatestLogoutTime { get; set; }
        public DateTime? LatestDeathEventTime { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }
        public int? SessionKills { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Headshots { get; set; }
        public int TeamKills { get; set; }
        public int Suicides { get; set; }
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
        public double KillsPerMinute
        {
            get
            {
                return Math.Round((double)(Kills / 60.0), 2);
            }
        }

        public double? SessionKillsPerMinute
        {
            get
            {
                if (SessionKills != null && SessionDurationMinutes != null && SessionDurationMinutes > 0)
                {
                    return (double?)Math.Round(((decimal)SessionKills / (decimal)SessionDurationMinutes), 2);
                }
                else
                {
                    return null;
                }
            }
        }

        public double HeadshotRatio
        {
            get
            {
                if (Kills > 0)
                {
                    return Math.Round((double)((double)Headshots / (double)Kills) * 100.0, 1);
                }
                else
                {
                    return 0;
                }
            }
        }

        public TimeSpan? SessionDurationRaw
        {
            get
            {
                return GetSessionDuration();
                //if (LatestLoginTime != null)
                //{

                //}
                //if (LatestLogoutTime == null)
                //{
                //    return (LatestLoginTime == null) ? null : (QueryStartTime - LatestLoginTime);
                //}

                //return (LatestLoginTime != null && LatestLogoutTime >= LatestLoginTime) ? (LatestLogoutTime - LatestLoginTime) : (QueryStartTime - LatestLoginTime);
            }
        }

        public int? SessionDurationMinutes
        {
            get
            {
                if (SessionDurationRaw == null)
                {
                    return null;
                }
                else
                {
                    return (int)SessionDurationRaw?.TotalMinutes;
                }
            }
        }

        public bool IsOnline
        {
            get
            {
                return GetIsOnline();
                //if ((LatestDeathEventTime > LatestLogoutTime) && LatestDeathEventTime != null && LatestLogoutTime != null)
                //{
                //    return true;
                //}
                //return ((LatestLogoutTime >= LatestLoginTime && LatestLogoutTime != null) || (LatestLogoutTime != null && LatestLoginTime == null)) ? false : true;
            }
        }

        private bool GetIsOnline()
        {
            // Whether we have a Login event that's more recent than the latest Logout event
            bool IsLoggedIn = (LatestLoginTime != null && ((LatestLogoutTime != null && LatestLoginTime > LatestLoginTime) || LatestLogoutTime == null));

            // Whether we have a Logout event that's more recent than the latest Login event
            bool IsLoggedOut = IsLoggedIn ? false : (LatestLogoutTime != null);

            bool NoInOutEvent = (IsLoggedIn == false && IsLoggedOut == false);

            DateTime? LatestInOutTime;

            if (IsLoggedIn == true)
            {
                LatestInOutTime = LatestLoginTime;
            }
            else if (IsLoggedOut == true)
            {
                LatestInOutTime = LatestLogoutTime;
            }
            else
            {
                LatestInOutTime = null;
            }

            //bool IsLatestInOutWithinHour = (LatestInOutTime != null && LatestInOutTime >= QueryStartTime);

            bool SeenInLastHour = (LatestDeathEventTime != null && LatestDeathEventTime >= QueryStartTime);

            bool AnyDeathAfterLatestInOut = (LatestDeathEventTime != null && ((LatestInOutTime != null && LatestDeathEventTime > LatestInOutTime) || LatestInOutTime == null));


            if (IsLoggedOut == true)
            {
                return (SeenInLastHour == true && AnyDeathAfterLatestInOut == true)
                    ? true
                    : false;
            }
            else
            {
                return (IsLoggedIn == true)
                    ? true
                    : AnyDeathAfterLatestInOut;
            }
        }

        private TimeSpan? GetSessionDuration()
        {
            if (LatestLoginTime != null)
            {
                DateTime sessionStartTime = (LatestLoginTime ?? QueryStartTime); //(playerStats.LatestLoginTime != null) ? (playerStats.LatestLoginTime ?? startTime) : startTime;
                DateTime sessionEndTime = (LatestLogoutTime ?? QueryNowUtc); // (playerStats.LatestLogoutTime != null) ? (playerStats.LatestLogoutTime ?? nowUtc) : nowUtc;

                if (sessionEndTime <= sessionStartTime)
                {
                    sessionEndTime = QueryNowUtc;
                }

                return (sessionEndTime - sessionStartTime);
            }
            else return null;
        }
    }
}
