using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class CharacterLifetimeStat
    {
        [Required]
        public string CharacterId { get; set; }

        public int? AchievementCount { get; set; }
        public int? AssistCount { get; set; }
        public int? FacilityDefendedCount { get; set; }
        public int? MedalCount { get; set; }
        public int? SkillPoints { get; set; }
        public int? WeaponDeaths { get; set; }
        public int? WeaponFireCount { get; set; }
        public int? WeaponHitCount { get; set; }
        public int? WeaponPlayTime { get; set; }
        public int? WeaponScore { get; set; }
    }
}
