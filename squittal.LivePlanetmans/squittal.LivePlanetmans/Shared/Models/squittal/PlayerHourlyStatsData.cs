using System;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerHourlyStatsData
    {
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
                if (LatestLoginTime != null && LatestLoginTime > QueryStartTime && LatestLogoutTime < QueryStartTime)
                {
                    return Math.Round((double)(Kills / (QueryNowUtc - (LatestLoginTime ?? QueryStartTime)).TotalMinutes), 2);
                }
                else
                {
                    return Math.Round((double)(Kills / 60.0), 2);
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

        /*
           === Session Stats ===
        */
        public DateTime SessionStartTime
        {
            get
            {
                return LatestLoginTime ?? QueryStartTime;
            }
        }

        public DateTime SessionEndTime
        {
            get
            {
                return GetResolvedEndTime();
            }
        }

        public TimeSpan SessionDurationRaw
        {
            get
            {
                return GetSessionDurationRaw();
            }
        }

        public int SessionDurationMinutes
        {
            get
            {
                return (int)SessionDurationRaw.TotalMinutes;
            }
        }

        public double? SessionKillsPerMinute
        {
            get
            {
                if (SessionKills != null && SessionDurationMinutes > 0)
                {
                    return (double?)Math.Round(((decimal)SessionKills / (decimal)SessionDurationMinutes), 2);
                }
                else
                {
                    return null;
                }
            }
        }
        
        public string SessionDurationDisplay
        {
            get
            {
                return GetSessionDurationDisplay();
            }
        }

        public string SessionBookendTimesDisplay
        {
            get
            {
                return GetDisplayBookendTimes();
            }
        }

        public bool IsOnline
        {
            get
            {
                return GetIsOnline();
            }
        }

        private bool GetIsOnline()
        {
            // Whether we have a Login event that's more recent than the latest Logout event
            bool IsLoggedIn = (LatestLoginTime != null && ((LatestLogoutTime != null && LatestLoginTime > LatestLoginTime) || LatestLogoutTime == null));

            // Whether we have a Logout event that's more recent than the latest Login event
            bool IsLoggedOut = IsLoggedIn ? false : (LatestLogoutTime != null);

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

        private DateTime GetResolvedEndTime()
        {
            var sessionEndTime = (LatestLogoutTime ?? QueryNowUtc);

            if (sessionEndTime <= SessionStartTime)
            {
                sessionEndTime = QueryNowUtc;
            }
            return sessionEndTime;
        }

        private TimeSpan GetSessionDurationRaw()
        {
            return SessionEndTime - SessionStartTime;
        }

        private string GetSessionDurationDisplay()
        {
            var totalMinutes = SessionDurationMinutes;

            int hours = (totalMinutes / 60);

            var remainder = totalMinutes - (hours * 60);

            string hoursDisplay = (hours > 0) ? $"{hours}h" : string.Empty;
            string minutesDisplay = (remainder > 0) ? $"{remainder}m" : string.Empty;
            string space = (hours > 0 && remainder > 0) ? " " : string.Empty;

            return $"{hoursDisplay}{space}{minutesDisplay}";
        }

        private string GetDisplayBookendTimes()
        {
            var startTime = SessionStartTime;
            var endTime = SessionEndTime;

            bool endIsNow = (endTime == QueryNowUtc);
            bool sameDates = (startTime.Date == endTime.Date);

            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);

            var startTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(startTime, TimeZoneInfo.Local);
            var endTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(endTime, TimeZoneInfo.Local);

            if (sameDates == true)
            {
                return endIsNow
                    ? $"{startTimeLocal.ToShortTimeString()} - Now"
                    : $"{startTimeLocal.ToShortTimeString()} - {endTimeLocal.ToShortTimeString()}";
            }
            else
            {
                return endIsNow
                    ? $"{startTimeLocal.ToString("M")} {startTimeLocal.ToShortTimeString()} - Now"
                    : $"{startTimeLocal.ToString("M")} {startTimeLocal.ToShortTimeString()} - {endTimeLocal.ToString("M")} {endTimeLocal.ToShortTimeString()}";
            }
        }
    }
}
