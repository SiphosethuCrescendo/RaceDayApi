using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.Entities
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public int CategoryID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public int? RaceNumber { get; set; }
        public string TShirtSize { get; set; } = string.Empty;

        // Navigation properties
        public Participant Participant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Result Result { get; set; } = null!;
    }
}