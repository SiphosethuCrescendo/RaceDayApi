using System;
using System.Collections.Generic;

namespace RaceDay.Core.Entities
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime RegistrationStartDate { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RouteDescription { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Organiser Organiser { get; set; } = null!;
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}