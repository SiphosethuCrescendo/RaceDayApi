using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        private int CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim == null)
                {
                    return 0;
                }
                return int.Parse(idClaim.Value);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            List<Event> events = await _eventService.GetAllAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            Event foundEvent = await _eventService.GetByIdAsync(id);
            if (foundEvent == null)
            {
                return NotFound(new { message = "Event not found" });
            }
            return Ok(foundEvent);
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            try
            {
                Event newEvent = await _eventService.CreateAsync(dto, CurrentUserId);
                return Ok(newEvent);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
        {
            try
            {
                Event updated = await _eventService.UpdateAsync(id, dto, CurrentUserId);
                if (updated == null)
                {
                    return NotFound(new { message = "Event not found or you do not own it" });
                }
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _eventService.DeleteAsync(id, CurrentUserId);
            if (deleted == false)
            {
                return NotFound(new { message = "Event not found or you do not own it" });
            }
            return Ok(new { message = "Event deleted" });
        }

        // NEW ENDPOINT FOR IMAGE UPLOAD
        [HttpPost("upload-image")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { message = "No image file provided" });
            }

            using var stream = imageFile.OpenReadStream();
            string imageUrl = await _eventService.UploadImageAsync(stream, imageFile.FileName);

            return Ok(new { url = imageUrl });
        }
    }
}