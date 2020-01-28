using System;
using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class ApplicationStart
    {
        [Required]
        public int ApplicationStartId { get; set; }

        [Required]
        public DateTime StartTimeUtc { get; set; }
    }
}
