using System;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class DbQueryTimes
    {
        public DateTime QueryStartTimeUtc { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public TimeSpan QueryTimeSpan {
            get
            {
                return QueryNowUtc - QueryStartTimeUtc;
            }
        }

        public string RangeDisplayLocalized
        {
            get
            {
                return GetLocalizedDateRangeDisplay();
            }
        }


        private string GetLocalizedDateRangeDisplay()
        {
            if (QueryStartTimeUtc == null || QueryNowUtc == null)
            {
                return string.Empty;
            }

            bool sameDates = (QueryStartTimeUtc.Date == QueryNowUtc.Date);
            
            var offsetHours = TimeSpan.FromHours(5); //TODO: use actual localization

            return sameDates
                    ? $"{GetOffsetShortTimeString(QueryStartTimeUtc, offsetHours)} - {GetOffsetShortTimeString(QueryNowUtc, offsetHours)}"
                    : $"{GetOffsetLongTimeString(QueryStartTimeUtc, offsetHours)} - {GetOffsetLongTimeString(QueryNowUtc, offsetHours)}";
        }

        private string GetOffsetShortTimeString(DateTime time, TimeSpan offsetHours)
        {
            return (time - offsetHours).ToShortTimeString();
        }

        private string GetOffsetLongTimeString(DateTime time, TimeSpan offsetHours)
        {
            return $"{(time - offsetHours).ToString("M")} {(time - offsetHours).ToShortTimeString()}";
        }
    }
}
