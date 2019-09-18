using System;
using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class OutfitMember
    {
        [Required]
        public string CharacterId { get; set; }
        [Required]
        public string OutfitId { get; set; }

        public DateTime? MemberSinceDate { get; set; }
        public string Rank { get; set; }
        public int? RankOrdinal { get; set; }

        public Character Character { get; set; }
        public Outfit Outfit { get; set; }
    }
}
