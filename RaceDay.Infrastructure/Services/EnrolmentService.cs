using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Infrastructure.Services
{
    public class EnrolmentService : IEnrolmentService
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<Enrolment> EnrolAsync(CreateEnrolmentDto dto, int participantId)
        {
            Participant participant = await _context.Participants.FirstOrDefaultAsync(p => p.ParticipantID == participantId);


            if (participant == null)
            {
                throw new Exception("Participant not found");
            }

            Category category = await _context.Categories
                .Include(c => c.Enrolments).FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID);


            if (category == null)
            {
                throw new Exception("Category not found");
            }

            int currentCount = category.Enrolments.Count;
            if (currentCount >= category.MaxParticipants)
            {
                throw new Exception("This category is full");
            }

            Enrolment alreadyEnrolled = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantID == participantId && e.CategoryID == dto.CategoryID);


            if (alreadyEnrolled != null)
            {
                throw new Exception("You are already enrolled in this category");
            }

            if (dto.EmergencyContactName != null && dto.EmergencyContactName != "")
            {
                participant.EmergencyContactName = dto.EmergencyContactName;
            }
            if (dto.EmergencyContactNumber != null && dto.EmergencyContactNumber != "")
            {
                participant.EmergencyContactNumber = dto.EmergencyContactNumber;
            }
            if (dto.MedicalConditions != null && dto.MedicalConditions != "")
            {
                participant.MedicalConditions = dto.MedicalConditions;
            }

            Enrolment newEnrolment = new Enrolment();
            newEnrolment.ParticipantID = participantId;
            newEnrolment.CategoryID = dto.CategoryID;
            newEnrolment.RegistrationDate = DateTime.Now;
            newEnrolment.Status = "Registered";
            newEnrolment.PaymentStatus = "Pending";
            newEnrolment.TShirtSize = dto.TShirtSize;

            _context.Enrolments.Add(newEnrolment);
            await _context.SaveChangesAsync();

            return newEnrolment;
        }

        public async Task<List<Enrolment>> GetParticipantEnrolmentsAsync(int participantId)
        {
            List<Enrolment> enrolments = await _context.Enrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c.Event)
                    .Include(e => e.Participant)
                    .Include(e => e.Category).ThenInclude(c => c.Event)
                .Include(e => e.Result)
                .Where(e => e.ParticipantID == participantId)
                .ToListAsync();

            return enrolments;
        }

        public async Task<List<Enrolment>> GetEventEnrolmentsAsync(int eventId, int organiserId)
        {
            Event eventItem = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId && e.OrganiserID == organiserId);


            if (eventItem == null)
            {
                throw new Exception("You do not own this event");
            }

            List<Enrolment> enrolments = await _context.Enrolments
                .Include(e => e.Category)
                .Include(e => e.Participant)
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .Include(e => e.Participant)
                    .ThenInclude(p => p.User)
                .Include(e => e.Result)
                .Where(e => e.Category.EventID == eventId)
                .ToListAsync();

            return enrolments;
        }

        public async Task<bool> UpdateStatusAsync(int enrolmentId, string status, int organiserId)
        {
            if (status != "Registered" && status != "Confirmed"
                && status != "Cancelled" && status != "Completed")
            {
                throw new Exception("Invalid status value");
            }

            Enrolment enrolment = await _context.Enrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c.Event).FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);


            if (enrolment == null)
            {
                return false;
            }

            if (enrolment.Category.Event.OrganiserID != organiserId)
            {
                throw new Exception("You don't own this event");
            }

            enrolment.Status = status;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}