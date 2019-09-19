using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusServices.Models
{
    public class CensusFactionModel
    {
        public int FactionId { get; set; }
        public MultiLanguageString Name { get; set; }
        public int ImageId { get; set; }
        public string CodeTag { get; set; }
        public bool UserSelectable { get; set; }
    }
}
