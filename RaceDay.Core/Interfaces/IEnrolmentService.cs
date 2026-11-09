using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceDay.Core.Entities;
using RaceDay.Core.DTOs;

namespace RaceDay.Core.Interfaces
{
    public interface IEnrolmentService
    {
        Task<Enrolment> EnrolAsync(CreateEnrolmentDto dto, int participantId);
        Task<List<Enrolment>> GetParticipantEnrolmentsAsync(int participantId);
        Task<List<Enrolment>> GetEventEnrolmentsAsync(int eventId, int organiserId);
        Task<bool> UpdateStatusAsync(int enrolmentId, string status, int organiserId);
    }
}
