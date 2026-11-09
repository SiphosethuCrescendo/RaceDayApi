using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentServiceTests
    {
        // Helper: create an Organiser + Event + Category in the fake database
        private async Task<Category> CreateTestCategory(RaceDayDbContext context, int maxParticipants)
        {
            // Create organiser user
            User orgUser = new User();
            orgUser.Email = "org@test.com";
            orgUser.PasswordHash = "hashed";
            orgUser.FullName = "Test Organiser";
            orgUser.PhoneNumber = "0821234567";
            orgUser.Role = "Organiser";
            orgUser.CreatedAt = DateTime.Now;
            orgUser.IsActive = true;
            context.Users.Add(orgUser);
            await context.SaveChangesAsync();

            Organiser organiser = new Organiser();
            organiser.OrganiserID = orgUser.UserID;
            organiser.CompanyName = "Test Co";
            organiser.CompanyPhone = "0215551234";
            organiser.ApprovedEvents = 0;
            context.Organisers.Add(organiser);

            // Create event
            Event newEvent = new Event();
            newEvent.OrganiserID = organiser.OrganiserID;
            newEvent.EventName = "Test Event";
            newEvent.Description = "Test";
            newEvent.EventDate = DateTime.Now.AddMonths(6);
            newEvent.RegistrationStartDate = DateTime.Now;
            newEvent.RegistrationEndDate = DateTime.Now.AddMonths(5);
            newEvent.Location = "Test";
            newEvent.City = "Cape Town";
            newEvent.Province = "Western Cape";
            newEvent.MaxParticipants = 100;
            newEvent.Status = "Published";
            newEvent.RouteDescription = "Route";
            newEvent.CreatedAt = DateTime.Now;
            newEvent.UpdatedAt = DateTime.Now;
            context.Events.Add(newEvent);
            await context.SaveChangesAsync();

            // Create category
            Category category = new Category();
            category.EventID = newEvent.EventID;
            category.CategoryName = "10km";
            category.Distance = 10;
            category.EntryFee = 100;
            category.MaxParticipants = maxParticipants;
            category.StartTime = new TimeSpan(7, 0, 0);
            category.AgeRestriction = 16;
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return category;
        }

        // Helper: create a Participant in the fake database
        private async Task<int> CreateTestParticipant(RaceDayDbContext context)
        {
            User user = new User();
            user.Email = "part@test.com";
            user.PasswordHash = "hashed";
            user.FullName = "Test Participant";
            user.PhoneNumber = "0821234567";
            user.Role = "Participant";
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
            context.Users.Add(user);
            await context.SaveChangesAsync();

            Participant participant = new Participant();
            participant.ParticipantID = user.UserID;
            context.Participants.Add(participant);
            await context.SaveChangesAsync();

            return user.UserID;
        }

        // ---- Success case: Participant enrols ----
        [Fact]
        public async Task Enrol_ValidParticipant_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);
            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";
            dto.EmergencyContactName = "Mom";
            dto.EmergencyContactNumber = "0829999999";
            dto.MedicalConditions = "None";

            Enrolment result = await service.EnrolAsync(dto, participantId);

            Assert.NotNull(result);
            Assert.Equal("Registered", result.Status);
            Assert.Equal("Pending", result.PaymentStatus);

            int count = await context.Enrolments.CountAsync();
            Assert.Equal(1, count);
        }

        // ---- Failure case: Enrol in a full category ----
        [Fact]
        public async Task Enrol_FullCategory_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            // Category with max 1 seat
            Category category = await CreateTestCategory(context, 1);
            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";

            // First enrolment succeeds
            await service.EnrolAsync(dto, participantId);

            // Second participant tries — should fail
            int secondParticipantId = await CreateTestParticipant(context);

            await Assert.ThrowsAsync<Exception>(() => service.EnrolAsync(dto, secondParticipantId));
        }

        // ---- Failure case: Duplicate enrolment ----
        [Fact]
        public async Task Enrol_DuplicateEnrolment_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);
            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";

            await service.EnrolAsync(dto, participantId);

            // Same participant, same category — should fail
            await Assert.ThrowsAsync<Exception>(() => service.EnrolAsync(dto, participantId));
        }

        // ---- Failure case: Unknown category ----
        [Fact]
        public async Task Enrol_UnknownCategory_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = 999; // does not exist
            dto.TShirtSize = "M";

            await Assert.ThrowsAsync<Exception>(() => service.EnrolAsync(dto, participantId));
        }

        // ---- Failure case: Unknown participant ----
        [Fact]
        public async Task Enrol_UnknownParticipant_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";

            await Assert.ThrowsAsync<Exception>(() => service.EnrolAsync(dto, 999));
        }

        // ---- Success case: Participant views own enrolments ----
        [Fact]
        public async Task GetParticipantEnrolments_ReturnsOwnEnrolments()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);
            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";
            await service.EnrolAsync(dto, participantId);

            var enrolments = await service.GetParticipantEnrolmentsAsync(participantId);

            Assert.Single(enrolments);
        }

        // ---- Failure case: Organiser cannot see other organisers' event enrolments ----
        [Fact]
        public async Task GetEventEnrolments_WrongOrganiser_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);

            // Try with a non-owner organiser ID
            await Assert.ThrowsAsync<Exception>(
                () => service.GetEventEnrolmentsAsync(category.EventID, 999));
        }

        // ---- Success case: Owner organiser views event enrolments ----
        [Fact]
        public async Task GetEventEnrolments_OwnEvent_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);

            // Find the organiser that owns the event
            var eventItem = await context.Events.FirstOrDefaultAsync(e => e.EventID == category.EventID);
            Assert.NotNull(eventItem);

            var result = await service.GetEventEnrolmentsAsync(category.EventID, eventItem.OrganiserID);

            Assert.NotNull(result);
        }

        // ---- Failure case: Update status with invalid value ----
        [Fact]
        public async Task UpdateStatus_InvalidStatus_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);
            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";
            Enrolment enrolment = await service.EnrolAsync(dto, participantId);

            var eventItem = await context.Events.FirstOrDefaultAsync(e => e.EventID == category.EventID);
            Assert.NotNull(eventItem);

            await Assert.ThrowsAsync<Exception>(
                () => service.UpdateStatusAsync(enrolment.EnrolmentID, "WrongStatus", eventItem.OrganiserID));
        }

        // ---- Success case: Owner organiser updates enrolment status ----
        [Fact]
        public async Task UpdateStatus_OwnerOrganiser_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EnrolmentService service = new EnrolmentService(context);

            Category category = await CreateTestCategory(context, 100);
            int participantId = await CreateTestParticipant(context);

            CreateEnrolmentDto dto = new CreateEnrolmentDto();
            dto.CategoryID = category.CategoryID;
            dto.TShirtSize = "M";
            Enrolment enrolment = await service.EnrolAsync(dto, participantId);

            var eventItem = await context.Events.FirstOrDefaultAsync(e => e.EventID == category.EventID);
            Assert.NotNull(eventItem);

            bool updated = await service.UpdateStatusAsync(
                enrolment.EnrolmentID, "Confirmed", eventItem.OrganiserID);

            Assert.True(updated);

            var refreshed = await context.Enrolments.FindAsync(enrolment.EnrolmentID);
            Assert.Equal("Confirmed", refreshed.Status);
        }
    }
}