using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlanetsideStatsSession
    {
        public DateTime StartTime
        {
            get
            {
                return GetResolvedStartTime();
            }
        }
        public DateTime EndTime
        {
            get
            {
                return GetResolvedEndTime();
            }
        }
        public string DisplayTimeBookends
        {
            get
            {
                return GetDisplayBookendTimes();
            }
        }

        public TimeSpan DurationRaw
        {
            get
            {
                return GetDurationRaw();
            }
        }

        public string DurationDisplay
        {
            get
            {
                return GetDurationDisplay();
            }
        }

        public int DurationMinutes
        {
            get
            {
                return GetDurationMinutes();
            }
        }

        public DeathEventAggregate Stats { get; }

        public double? KillsPerMinute
        {
            get
            {
                if (DurationMinutes > 0)
                {
                    return Math.Round((double)(Stats.Kills / DurationMinutes), 2);
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly PlayerHourlyStatsData _inputStats;


        public PlanetsideStatsSession(PlayerHourlyStatsData inputStats)
        {
            _inputStats = inputStats;

            Stats.Kills = _inputStats.SessionKills ?? 0;
        }


        private DateTime GetResolvedStartTime()
        {
            return (_inputStats.LatestLoginTime ?? _inputStats.QueryStartTime);
        }

        private DateTime GetResolvedEndTime()
        {
            var sessionEndTime = (_inputStats.LatestLogoutTime ?? _inputStats.QueryNowUtc);

            if (sessionEndTime <= StartTime)
            {
                sessionEndTime = _inputStats.QueryNowUtc;
            }
            return sessionEndTime;
        }

        private TimeSpan GetDurationRaw()
        {
            return (EndTime - StartTime);
        }

        private int GetDurationMinutes()
        {
            return (int)Math.Round(DurationRaw.TotalMinutes, 0);
        }

        private string GetDurationDisplay()
        {
            var totalMinutes = DurationMinutes;

            int hours = (totalMinutes / 60);

            var remainder = totalMinutes - (hours * 60);

            string hoursDisplay = (hours > 0) ? $"{hours}h" : string.Empty;
            string minutesDisplay = (remainder > 0) ? $"{remainder}m" : string.Empty;
            string space = (hours > 0 && remainder > 0) ? " " : string.Empty;

            return $"{hoursDisplay}{space}{minutesDisplay}";
        }

        private string GetDisplayBookendTimes()
        {
            var startTime = StartTime;
            var endTime = EndTime;

            bool endIsNow = (endTime == _inputStats.QueryNowUtc);
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
