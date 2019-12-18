using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlanetsideSessionTimes
    {
        public DateTime StartTime => GetResolvedStartTime();
        public DateTime EndTime => GetResolvedEndTime();
        public string DisplayTimeBookends => GetDisplayBookendTimes();

        public TimeSpan DurationRaw => GetDurationRaw();
        public int DurationMinutes => GetDurationMinutes();
        public string DurationDisplay => GetDurationDisplay();


        private readonly PlayerHourlyStatsData _inputStats;

        public PlanetsideSessionTimes(PlayerHourlyStatsData inputStats)
        {
            _inputStats = inputStats;
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
