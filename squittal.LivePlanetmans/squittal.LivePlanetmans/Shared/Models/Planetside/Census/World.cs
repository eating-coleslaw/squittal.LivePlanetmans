using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class World
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
