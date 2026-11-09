using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class EventModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string City { get; set; } = "";
        public string Province { get; set; } = "";
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int OrganiserID { get; set; }
        public List<CategoryModel> Categories { get; set; } = new();
    }

    public class CategoryModel
       
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
        public EventModel? Event { get; set; } 
    }

    public class EnrolmentModel
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string Status { get; set; } = "Pending";
        public CategoryModel? Category { get; set; }
    }

    public class ResultModel
    {
        public int ResultID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public EnrolmentModel? Enrolment { get; set; }
    }

    public class UserModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }

    public class EventFormViewModel
    {
        public int EventID { get; set; }
        [Required] public string Name { get; set; } = "";
        [Required] public string Description { get; set; } = "";
        [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);
        [Required] public string Location { get; set; } = "";
        public decimal Distance { get; set; }
        public string EventType { get; set; } = "Run";
        public string? ImageUrl { get; set; }
    }

    public class DashboardRow
    {
        public EventModel Event { get; set; } = new();
        public int Enrolments { get; set; }
    }

    public class ResultRow
    {
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Category { get; set; } = "";
        public string FinishTime { get; set; } = "";
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }
        public static string Ordinal(int n) => (n % 100 is 11 or 12 or 13) ? $"{n}th" : (n % 10) switch
        { 1 => $"{n}st", 2 => $"{n}nd", 3 => $"{n}rd", _ => $"{n}th" };
    }
}