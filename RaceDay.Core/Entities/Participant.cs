using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.Entities
{
    public class Participant
    {
        public int ParticipantID { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactNumber { get; set; } = string.Empty;
        public string MedicalConditions { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;

        // Navigation properties
        public User User { get; set; } = null!;
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}