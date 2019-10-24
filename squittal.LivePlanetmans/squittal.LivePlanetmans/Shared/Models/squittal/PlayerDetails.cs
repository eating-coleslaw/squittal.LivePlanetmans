using System;
using System.Collections.Generic;
using System.Text;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerDetails
    {
        public string PlayerId { get; set; }
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryNowUtc { get; set; }

        public string PlayerName { get; set; }
        public int BattleRank { get; set; }
        public int PrestigeLevel { get; set; }
        public string TitleName { get; set; }

        public int FactionId { get; set; }
        public string FactionName { get; set; }

        public int WorldId { get; set; }
        public string WorldName { get; set; }

        public string OutfitId { get; set; }
        public string OutfitName { get; set; }
        public string OutfitAlias { get; set; }
        public string OutfitRankName { get; set; }
        
        

    }
}
