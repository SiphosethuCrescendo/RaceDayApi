using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class EventServiceTests
    {
        // Helper to create an Organiser in the fake database
        private async Task<int> CreateTestOrganiser(RaceDayDbContext context)
        {
            User user = new User();
            user.Email = "org@test.com";
            user.PasswordHash = "hashed";
            user.FullName = "Test Organiser";
            user.PhoneNumber = "0821234567";
            user.Role = "Organiser";
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            Organiser organiser = new Organiser();
            organiser.OrganiserID = user.UserID;
            organiser.CompanyName = "Test Events Co";
            organiser.CompanyPhone = "0215551234";
            organiser.ApprovedEvents = 0;

            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();

            return user.UserID;
        }

        private CreateEventDto BuildEventDto()
        {
            CreateEventDto dto = new CreateEventDto();
            dto.EventName = "Test Marathon";
            dto.Description = "A test event";
            dto.EventDate = DateTime.Now.AddMonths(6);
            dto.RegistrationStartDate = DateTime.Now;
            dto.RegistrationEndDate = DateTime.Now.AddMonths(5);
            dto.Location = "Test Stadium";
            dto.City = "Cape Town";
            dto.Province = "Western Cape";
            dto.MaxParticipants = 1000;
            dto.Status = "Published";
            dto.RouteDescription = "Flat route";
            return dto;
        }

        // ---- Success case: Organiser creates an event ----
        [Fact]
        public async Task Create_ValidEvent_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);

            int organiserId = await CreateTestOrganiser(context);
            CreateEventDto dto = BuildEventDto();

            Event result = await service.CreateAsync(dto, organiserId);

            Assert.NotNull(result);
            Assert.Equal("Test Marathon", result.EventName);
            Assert.Equal(organiserId, result.OrganiserID);

            int count = await context.Events.CountAsync();
            Assert.Equal(1, count);
        }

        // ---- Failure case: Bad registration dates ----
        [Fact]
        public async Task Create_StartAfterEnd_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);

            int organiserId = await CreateTestOrganiser(context);
            CreateEventDto dto = BuildEventDto();
            dto.RegistrationStartDate = DateTime.Now.AddMonths(5);
            dto.RegistrationEndDate = DateTime.Now;

            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(dto, organiserId));
        }

        // ---- Failure case: Unknown organiser ----
        [Fact]
        public async Task Create_UnknownOrganiser_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);

            CreateEventDto dto = BuildEventDto();

            // Organiser 999 does not exist
            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(dto, 999));
        }

        // ---- Success case: Get all events ----
        [Fact]
        public async Task GetAll_ReturnsAllEvents()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);
            int organiserId = await CreateTestOrganiser(context);

            await service.CreateAsync(BuildEventDto(), organiserId);

            CreateEventDto dto2 = BuildEventDto();
            dto2.EventName = "Second Event";
            await service.CreateAsync(dto2, organiserId);

            List<Event> events = await service.GetAllAsync();

            Assert.Equal(2, events.Count);
        }

        // ---- Failure case: Participant tries to modify another organiser's event ----
        [Fact]
        public async Task Update_WrongOrganiser_ReturnsNull()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);
            int organiserId = await CreateTestOrganiser(context);

            Event created = await service.CreateAsync(BuildEventDto(), organiserId);

            UpdateEventDto update = new UpdateEventDto();
            update.EventName = "Hacked Name";
            update.Description = "Hacked";
            update.EventDate = DateTime.Now.AddMonths(6);
            update.RegistrationStartDate = DateTime.Now;
            update.RegistrationEndDate = DateTime.Now.AddMonths(5);
            update.Location = "X";
            update.City = "X";
            update.Province = "X";
            update.MaxParticipants = 100;
            update.Status = "Published";
            update.RouteDescription = "X";

            // Try with a different organiser ID (999)
            Event result = await service.UpdateAsync(created.EventID, update, 999);

            Assert.Null(result);
        }

        // ---- Success case: Owner can update their own event ----
        [Fact]
        public async Task Update_CorrectOrganiser_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);
            int organiserId = await CreateTestOrganiser(context);

            Event created = await service.CreateAsync(BuildEventDto(), organiserId);

            UpdateEventDto update = new UpdateEventDto();
            update.EventName = "Updated Name";
            update.Description = "Updated";
            update.EventDate = DateTime.Now.AddMonths(6);
            update.RegistrationStartDate = DateTime.Now;
            update.RegistrationEndDate = DateTime.Now.AddMonths(5);
            update.Location = "Updated Location";
            update.City = "Updated City";
            update.Province = "Updated Province";
            update.MaxParticipants = 2000;
            update.Status = "Published";
            update.RouteDescription = "Updated route";

            Event result = await service.UpdateAsync(created.EventID, update, organiserId);

            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.EventName);
        }

        // ---- Failure case: Delete an event owned by someone else ----
        [Fact]
        public async Task Delete_WrongOrganiser_ReturnsFalse()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);
            int organiserId = await CreateTestOrganiser(context);

            Event created = await service.CreateAsync(BuildEventDto(), organiserId);

            bool deleted = await service.DeleteAsync(created.EventID, 999);

            Assert.False(deleted);
        }

        //  Success case: Owner deletes their own event 
        [Fact]
        public async Task Delete_CorrectOrganiser_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            EventService service = new EventService(context);
            int organiserId = await CreateTestOrganiser(context);

            Event created = await service.CreateAsync(BuildEventDto(), organiserId);

            bool deleted = await service.DeleteAsync(created.EventID, organiserId);

            Assert.True(deleted);

            int count = await context.Events.CountAsync();
            Assert.Equal(0, count);
        }
    }
}