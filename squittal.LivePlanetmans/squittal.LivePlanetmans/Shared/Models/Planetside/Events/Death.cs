using System;
using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class Death
    {
        [Required]
        public string CharacterId { get; set; }
        [Required]
        public string AttackerCharacterId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }

        public int WorldId { get; set; }
        public int ZoneId { get; set; }
        public int? CharacterLoadoutId { get; set; }
        public int? AttackerFireModeId { get; set; }
        public int? AttackerLoadoutId { get; set; }
        public int? AttackerVehicleId { get; set; }
        public int? AttackerWeaponId { get; set; }
        public int? VehicleId { get; set; }
        public bool IsHeadshot { get; set; }
        public bool IsCritical { get; set; }
    }
}
