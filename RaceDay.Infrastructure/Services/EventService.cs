using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly RaceDayDbContext _context;

        public EventService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<List<Event>> GetAllAsync()
        {
            return await _context.Events.Include(e => e.Categories).Include(e => e.Organiser).ToListAsync();
        }

        public async Task<Event> GetByIdAsync(int id)
        {
            return await _context.Events.Include(e => e.Categories).Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
        }

        public async Task<Event> CreateAsync(CreateEventDto dto, int organiserId)
        {
            if (dto.RegistrationStartDate >= dto.RegistrationEndDate)
                throw new Exception("Registration start date must be before end date");

            if (dto.EventDate <= dto.RegistrationEndDate)
                throw new Exception("Event date must be after registration end date");

            var newEvent = new Event
            {
                OrganiserID = organiserId,
                EventName = dto.EventName,
                Description = dto.Description,
                EventDate = dto.EventDate,
                RegistrationStartDate = dto.RegistrationStartDate,
                RegistrationEndDate = dto.RegistrationEndDate,
                Location = dto.Location,
                City = dto.City,
                Province = dto.Province,
                MaxParticipants = dto.MaxParticipants,
                Status = dto.Status,
                RouteDescription = dto.RouteDescription,
                ImageUrl = dto.ImageUrl,
                Distance = dto.Distance,
                EventType = dto.EventType,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        public async Task<Event> UpdateAsync(int id, UpdateEventDto dto, int organiserId)
        {
            var existing = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id && e.OrganiserID == organiserId);
            if (existing == null) return null;

            if (dto.RegistrationStartDate >= dto.RegistrationEndDate)
                throw new Exception("Registration start date must be before end date");

            if (dto.EventDate <= dto.RegistrationEndDate)
                throw new Exception("Event date must be after registration end date");

            existing.EventName = dto.EventName;
            existing.Description = dto.Description;
            existing.EventDate = dto.EventDate;
            existing.RegistrationStartDate = dto.RegistrationStartDate;
            existing.RegistrationEndDate = dto.RegistrationEndDate;
            existing.Location = dto.Location;
            existing.City = dto.City;
            existing.Province = dto.Province;
            existing.MaxParticipants = dto.MaxParticipants;
            existing.Status = dto.Status;
            existing.RouteDescription = dto.RouteDescription;
            existing.ImageUrl = dto.ImageUrl;
            existing.Distance = dto.Distance;
            existing.EventType = dto.EventType;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, int organiserId)
        {
            var existing = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id && e.OrganiserID == organiserId);
            if (existing == null) return false;
            _context.Events.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            string connectionString = "UseDevelopmentStorage=true";
            string containerName = "event-images";
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
            BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);
            await blobClient.UploadAsync(fileStream, true);
            return blobClient.Uri.ToString();
        }
    }
}