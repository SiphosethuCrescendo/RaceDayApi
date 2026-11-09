using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;

namespace RaceDay.Core.Interfaces
{
    public interface IEventService
    {
        Task<List<Event>> GetAllAsync();
        Task<Event> GetByIdAsync(int id);
        Task<Event> CreateAsync(CreateEventDto dto, int organiserId);
        Task<Event> UpdateAsync(int id, UpdateEventDto dto, int organiserId);
        Task<bool> DeleteAsync(int id, int organiserId);
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
    }
}