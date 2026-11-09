using Microsoft.EntityFrameworkCore;
using RaceDay.Core.Entities;
using System;

namespace RaceDay.Infrastructure.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relationships & Constraints ---
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.OrganiserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.ParticipantID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(en => en.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Decimal Precision ---
            modelBuilder.Entity<Category>().Property(c => c.Distance).HasColumnType("decimal(5,2)");
            modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Event>().Property(e => e.Distance).HasColumnType("decimal(5,2)");

            // ==========================================
            // AUTOMATIC SEED DATA (Injects data on startup)
            // ==========================================

            // 1. Seed Organiser
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Email = "admin@raceday.co.za",
                    // Password is 'Password123!' (hashed)
                    PasswordHash = "$2a$11$3Q3k7wXqZ4V5yW8mN9pOe.5X7yZ8w9V0u1S2T3R4Q5P6O7N8M9L0K",
                    FullName = "RaceDay Admin",
                    PhoneNumber = "0821112222",
                    Role = "Organiser",
                    CreatedAt = new DateTime(2026, 1, 1),
                    IsActive = true
                }
            );

            modelBuilder.Entity<Organiser>().HasData(
                new Organiser
                {
                    OrganiserID = 1,
                    CompanyName = "South African Athletics",
                    CompanyPhone = "0215551234",
                    ApprovedEvents = 0
                }
            );

            // 2. Seed Events
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventID = 1,
                    OrganiserID = 1,
                    EventName = "Cape Town Cycle Tour",
                    Description = "The largest timed cycle race in South Africa.",
                    EventDate = new DateTime(2026, 3, 15, 7, 0, 0),
                    RegistrationStartDate = new DateTime(2025, 10, 1),
                    RegistrationEndDate = new DateTime(2026, 3, 1),
                    Location = "Cape Town Stadium",
                    City = "Cape Town",
                    Province = "Western Cape",
                    MaxParticipants = 35000,
                    Status = "Published",
                    RouteDescription = "Down to Muizenberg and back",
                    ImageUrl = "https://images.unsplash.com/photo-1541625602330-2277a4c46182?q=80&w=800&auto=format&fit=crop",
                    Distance = 109.0m,
                    EventType = "Cycle",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new Event
                {
                    EventID = 2,
                    OrganiserID = 1,
                    EventName = "Comrades Marathon",
                    Description = "The Ultimate Human Race.",
                    EventDate = new DateTime(2026, 6, 14, 5, 30, 0),
                    RegistrationStartDate = new DateTime(2025, 9, 1),
                    RegistrationEndDate = new DateTime(2026, 5, 1),
                    Location = "Pietermaritzburg City Hall",
                    City = "Pietermaritzburg",
                    Province = "KwaZulu-Natal",
                    MaxParticipants = 20000,
                    Status = "Published",
                    RouteDescription = "Between Pietermaritzburg and Durban",
                    ImageUrl = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?q=80&w=800&auto=format&fit=crop",
                    Distance = 89.0m,
                    EventType = "Run",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new Event
                {
                    EventID = 3,
                    OrganiserID = 1,
                    EventName = "Two Oceans Marathon",
                    Description = "The world's most beautiful marathon.",
                    EventDate = new DateTime(2026, 4, 4, 6, 0, 0),
                    RegistrationStartDate = new DateTime(2025, 11, 1),
                    RegistrationEndDate = new DateTime(2026, 3, 1),
                    Location = "UCT Campus",
                    City = "Cape Town",
                    Province = "Western Cape",
                    MaxParticipants = 11000,
                    Status = "Published",
                    RouteDescription = "Along the slopes of Table Mountain",
                    ImageUrl = "https://images.unsplash.com/photo-1533560904424-a0c61dc306fc?q=80&w=800&auto=format&fit=crop",
                    Distance = 56.0m,
                    EventType = "Run",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                },
                new Event
                {
                    EventID = 4,
                    OrganiserID = 1,
                    EventName = "Soweto Community Walk",
                    Description = "A family-friendly 5km walk through Soweto.",
                    EventDate = new DateTime(2026, 5, 20, 8, 0, 0),
                    RegistrationStartDate = new DateTime(2025, 12, 1),
                    RegistrationEndDate = new DateTime(2026, 5, 1),
                    Location = "Vilakazi Street",
                    City = "Soweto",
                    Province = "Gauteng",
                    MaxParticipants = 5000,
                    Status = "Published",
                    RouteDescription = "Flat route through historic streets",
                    ImageUrl = "https://images.unsplash.com/photo-1476480862126-209bfaa8edc8?q=80&w=800&auto=format&fit=crop",
                    Distance = 5.0m,
                    EventType = "Walk",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                }
            );

            // 3. Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryID = 1, EventID = 1, CategoryName = "Elite Men", Distance = 109.0m, EntryFee = 350.00m, MaxParticipants = 500, StartTime = new TimeSpan(7, 0, 0), AgeRestriction = 18 },
                new Category { CategoryID = 2, EventID = 1, CategoryName = "Elite Women", Distance = 109.0m, EntryFee = 350.00m, MaxParticipants = 500, StartTime = new TimeSpan(7, 10, 0), AgeRestriction = 18 },
                new Category { CategoryID = 3, EventID = 1, CategoryName = "Fun Ride", Distance = 109.0m, EntryFee = 250.00m, MaxParticipants = 3000, StartTime = new TimeSpan(7, 30, 0), AgeRestriction = 16 },
                new Category { CategoryID = 4, EventID = 2, CategoryName = "Ultra Men", Distance = 89.0m, EntryFee = 500.00m, MaxParticipants = 800, StartTime = new TimeSpan(5, 30, 0), AgeRestriction = 20 },
                new Category { CategoryID = 5, EventID = 2, CategoryName = "Ultra Women", Distance = 89.0m, EntryFee = 500.00m, MaxParticipants = 800, StartTime = new TimeSpan(5, 40, 0), AgeRestriction = 20 },
                new Category { CategoryID = 6, EventID = 3, CategoryName = "Half Marathon", Distance = 21.1m, EntryFee = 300.00m, MaxParticipants = 5000, StartTime = new TimeSpan(6, 0, 0), AgeRestriction = 18 },
                new Category { CategoryID = 7, EventID = 3, CategoryName = "Ultra Marathon", Distance = 56.0m, EntryFee = 450.00m, MaxParticipants = 2000, StartTime = new TimeSpan(5, 15, 0), AgeRestriction = 20 },
                new Category { CategoryID = 8, EventID = 4, CategoryName = "Open 5km", Distance = 5.0m, EntryFee = 50.00m, MaxParticipants = 1000, StartTime = new TimeSpan(8, 0, 0), AgeRestriction = null }
            );
        }
    }
}