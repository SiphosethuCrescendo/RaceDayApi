using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.DTOs
{
    // What the client sends to enrol in an event
    public class CreateEnrolmentDto
    {
        public int CategoryID { get; set; }
        public string TShirtSize { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactNumber { get; set; } = string.Empty;
        public string MedicalConditions { get; set; } = string.Empty;
    }
}
