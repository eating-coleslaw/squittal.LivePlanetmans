using System.ComponentModel.DataAnnotations;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class Faction
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
        public int? ImageId { get; set; }
        public string CodeTag { get; set; }
        public bool UserSelectable { get; set; }
    }
}
