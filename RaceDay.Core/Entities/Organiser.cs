using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.Entities
{
    public class Organiser
    {
        public int OrganiserID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public int ApprovedEvents { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
