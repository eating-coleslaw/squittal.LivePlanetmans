using System;
using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerSession
    {
        public int Id { get; set; }

        [Required]
        public string CharacterId { get; set; }

        public DateTime LoginDate { get; set; }
        public DateTime LogoutDate { get; set; }

        public int Duration { get; set; } // Total Milliseconds

        public int? Kills { get; set; }
        public int? Deaths { get; set; }
        public int? Headshots { get; set; }
        public int? Teamkills { get; set; }
        public int? Suicides { get; set; }
    }
}
