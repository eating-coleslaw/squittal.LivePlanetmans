using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class Title
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
