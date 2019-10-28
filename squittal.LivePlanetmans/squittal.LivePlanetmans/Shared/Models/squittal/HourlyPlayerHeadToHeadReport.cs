using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class HourlyPlayerHeadToHeadReport
    {
        public PlayerDetails PlayerDetails { get; set; }
        public DbQueryTimes QueryTimes { get; set; }

        public IEnumerable<PlayerHeadToHeadSummaryRow> HeadToHeadSummaries { get; set; }

        public PlayerHeadToHeadSummaryRow[] SortedHeadToHeadSummaries { get; set; }


        private SortColumn _defaultSortColumn = SortColumn.Kills;
        private SortDirection _defaultSortDirection = SortDirection.Descending;

        private SortColumn _sortColumn;
        private SortDirection _sortDirection;

        public HourlyPlayerHeadToHeadReport()
        {
            _sortColumn = _defaultSortColumn;
            _sortDirection = _defaultSortDirection;
        }

        public void SortReport(SortColumn column)
        {
            UpdateSortColumn(column);
            UpdateSortDirection(column);

            switch (column)
            {
                case SortColumn.Kills:
                    HeadToHeadSummaries = SortByKills();
                    break;

                case SortColumn.Deaths:
                    HeadToHeadSummaries = SortByDeaths();
                    break;

                case SortColumn.KillDeathRatio:
                    HeadToHeadSummaries = SortByKillDeathRatio();
                    break;

                case SortColumn.Player:
                    HeadToHeadSummaries = SortByPlayerName();
                    break;

                default:
                    HeadToHeadSummaries = SortByKills();
                    break;
            }
        }


        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByKills()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.PlayerStats.Kills) //.AsEnumerable()
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.PlayerStats.Kills); //.AsEnumerable()
        }

        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByDeaths()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.PlayerStats.Deaths)
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.PlayerStats.Deaths);
        }

        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByKillDeathRatio()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.PlayerStats.KillDeathRatio)
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.PlayerStats.KillDeathRatio);
        }

        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByPlayerName()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.EnemyDetails.PlayerName)
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.EnemyDetails.PlayerName);
        }


        private SortColumn UpdateSortColumn(SortColumn column)
        {
            _sortColumn = column;
            return _sortColumn;
        }

        private SortDirection UpdateSortDirection(SortColumn column)
        {
            _sortDirection = (column == _sortColumn)
                                ? GetOppositeSortDirection(_sortDirection)
                                : _defaultSortDirection;

            return _sortDirection;
        }

        private SortDirection GetOppositeSortDirection(SortDirection direction)
        {
            return direction == SortDirection.Ascending
                ? SortDirection.Descending
                : SortDirection.Ascending;
        }

    }

    public class PlayerHeadToHeadSummaryRow
    {
        public PlayerDetails EnemyDetails { get; set; }
        
        public DeathEventAggregate PlayerStats { get; set; }
        public DeathEventAggregate EnemyStats { get; set; }
    }

    public class HeadToHeadSummaryRow
    {
        public PlayerDetails AttackerDetails { get; set; }
        public PlayerDetails VictimDetails { get; set; }

        public DeathEventAggregate AttackerStats { get; set; }
        public DeathEventAggregate VictimStats { get; set; }
    }

    public enum SortDirection
    {
        Descending,
        Ascending
    }

    public enum SortColumn
    {
        Kills,
        Deaths,
        KillDeathRatio,
        Player
    }
}
