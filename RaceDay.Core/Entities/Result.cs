using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.Entities
{
    public class Result
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int? OverallRank { get; set; }
        public int? CategoryRank { get; set; }
        public TimeSpan? GunTime { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public string Status { get; set; } = string.Empty;

        // Navigation property
        public Enrolment Enrolment { get; set; } = null!;
    }
}