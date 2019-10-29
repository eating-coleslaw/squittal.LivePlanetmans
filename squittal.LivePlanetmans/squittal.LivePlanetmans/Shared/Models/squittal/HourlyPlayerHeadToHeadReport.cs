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

        private SortColumn _prevSortColumn;
        private SortDirection _prevSortDirection;

        public HourlyPlayerHeadToHeadReport()
        {
            //_sortColumn = _defaultSortColumn;
            //_sortDirection = _defaultSortDirection;
        }

        public HourlyPlayerHeadToHeadReport(PlayerDetails playerDetails, IEnumerable<PlayerHeadToHeadSummaryRow> summaries, DbQueryTimes dbQueryTimes)
        {
            //_sortColumn = _defaultSortColumn;
            //_sortDirection = _defaultSortDirection;

            PlayerDetails = playerDetails;
            HeadToHeadSummaries = summaries;
            QueryTimes = dbQueryTimes;
            SortByDefaults();
        }

        public void SortByDefaults()
        {
            _prevSortColumn = _sortColumn;
            _prevSortDirection = _sortDirection;
            
            SortByColumn(_defaultSortColumn, true);
        }

        public void SortByColumn(SortColumn column, bool forceDefaultDirection = false)
        {
            UpdateSortColumn(column);
            UpdateSortDirection(column, forceDefaultDirection);

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
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.PlayerStats.Kills).AsEnumerable()
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.PlayerStats.Kills).AsEnumerable();
        }

        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByDeaths()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.PlayerStats.Deaths).AsEnumerable()
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.PlayerStats.Deaths).AsEnumerable();
        }

        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByKillDeathRatio()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.PlayerStats.KillDeathRatio).AsEnumerable()
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.PlayerStats.KillDeathRatio).AsEnumerable();
        }

        private IEnumerable<PlayerHeadToHeadSummaryRow> SortByPlayerName()
        {
            return (_sortDirection == SortDirection.Ascending)
                ? HeadToHeadSummaries.OrderBy(h2h => h2h.EnemyDetails.PlayerName).AsEnumerable()
                : HeadToHeadSummaries.OrderByDescending(h2h => h2h.EnemyDetails.PlayerName).AsEnumerable();
        }


        private SortColumn UpdateSortColumn(SortColumn column)
        {
            _prevSortColumn = _sortColumn;

            _sortColumn = column;
            return _sortColumn;
        }

        private SortDirection UpdateSortDirection(SortColumn newColumn, bool forceDefaultDirection)
        {
            _prevSortDirection = _sortDirection;

            _sortDirection = (newColumn == _prevSortColumn && forceDefaultDirection != true)
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

    public class PlayerHeadToHeadSummaryRow : IEquatable<PlayerHeadToHeadSummaryRow>
    {
        public PlayerDetails EnemyDetails { get; set; }
        
        public DeathEventAggregate PlayerStats { get; set; }
        public DeathEventAggregate EnemyStats { get; set; }

        public string DebugString
        {
            get
            {
                return $"   vs {EnemyDetails.PlayerName}: k{PlayerStats.Kills} d{PlayerStats.Deaths}  |  k{EnemyStats.Kills} d{EnemyStats.Deaths}";
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as PlayerHeadToHeadSummaryRow);
        }

        public bool Equals(PlayerHeadToHeadSummaryRow r)
        {
            if (ReferenceEquals(r, null))
            {
                return false;
            }

            if (ReferenceEquals(this, r))
            {
                return true;
            }

            if (this.GetType() != r.GetType())
            {
                return false;
            }

            return (EnemyDetails.PlayerId == r.EnemyDetails.PlayerId) && (EnemyDetails.PlayerId == r.EnemyDetails.PlayerId);
        }

        public static bool operator ==(PlayerHeadToHeadSummaryRow lhs, PlayerHeadToHeadSummaryRow rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PlayerHeadToHeadSummaryRow lhs, PlayerHeadToHeadSummaryRow rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return EnemyDetails.PlayerId.GetHashCode(); // base.GetHashCode();
        }
    }

    public class HeadToHeadSummaryRow : IEquatable<HeadToHeadSummaryRow>
    {
        public PlayerDetails AttackerDetails { get; set; }
        public PlayerDetails VictimDetails { get; set; }

        public DeathEventAggregate AttackerStats { get; set; }
        public DeathEventAggregate VictimStats { get; set; }


        public string DebugString
        {
            get
            {
                return $"{AttackerDetails.PlayerName} vs {VictimDetails.PlayerName}: k{AttackerStats.Kills} d{AttackerStats.Deaths}";
            }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as HeadToHeadSummaryRow);
        }

        public bool Equals(HeadToHeadSummaryRow r)
        {
            if (ReferenceEquals(r, null))
            {
                return false;
            }

            if (ReferenceEquals(this, r))
            {
                return true;
            }

            if (this.GetType() != r.GetType())
            {
                return false;
            }

            return (AttackerDetails.PlayerId == r.AttackerDetails.PlayerId) && (VictimDetails.PlayerId == r.VictimDetails.PlayerId);
        }

        public static bool operator ==(HeadToHeadSummaryRow lhs, HeadToHeadSummaryRow rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(HeadToHeadSummaryRow lhs, HeadToHeadSummaryRow rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
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
