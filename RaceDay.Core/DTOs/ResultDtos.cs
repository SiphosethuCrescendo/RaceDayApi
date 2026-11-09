using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Core.DTOs
{
    // What the client sends to upload a result
    public class CreateResultDto
    {
        public int EnrolmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public TimeSpan? GunTime { get; set; }
        public TimeSpan? ChipTime { get; set; }
    }
}