using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.Entities
{
    public class Category
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
        public TimeSpan? StartTime { get; set; }
        public int? AgeRestriction { get; set; }

        // Navigation properties
        public Event Event { get; set; } = null!;
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}