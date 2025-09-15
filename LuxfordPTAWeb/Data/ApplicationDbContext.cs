using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<EventMainSponsor> EventMainSponsors { get; set; }
        public DbSet<EventOtherSponsor> EventOtherSponsors { get; set; }
        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventMainSponsor>()
                .HasKey(x => new { x.EventId, x.SponsorId });

            builder.Entity<EventMainSponsor>()
                .HasOne(x => x.Event)
                .WithMany(e => e.MainSponsors)
                .HasForeignKey(x => x.EventId);

            builder.Entity<EventMainSponsor>()
                .HasOne(x => x.Sponsor)
                .WithMany(s => s.MainEvents)
                .HasForeignKey(x => x.SponsorId);

            builder.Entity<EventOtherSponsor>()
                .HasKey(x => new { x.EventId, x.SponsorId });

            builder.Entity<EventOtherSponsor>()
                .HasOne(x => x.Event)
                .WithMany(e => e.OtherSponsors)
                .HasForeignKey(x => x.EventId);

            builder.Entity<EventOtherSponsor>()
                .HasOne(x => x.Sponsor)
                .WithMany(s => s.OtherEvents)
                .HasForeignKey(x => x.SponsorId);

            // Seed EventTypes with enhanced properties
            builder.Entity<EventType>().HasData(
                new EventType 
                { 
                    Id = 1, 
                    Name = "School Closed & Special Days", 
                    Slug = "school-closed-days",
                    Description = "All holidays, staff days, and special schedule days for the school year.", 
                    IsMandatory = true,
                    DisplayOrder = 1,
                    IsActive = true,
                    Size = EventTypeSize.Full,
                    Icon = "bi-calendar-x",
                    ColorClass = "text-danger"
                },
                new EventType 
                { 
                    Id = 2, 
                    Name = "Fundraising Events",
                    Slug = "fundraising-events",
                    Description = "Events focused on raising funds to support school programs, equipment, and activities.", 
                    IsMandatory = false,
                    DisplayOrder = 2,
                    IsActive = true,
                    Size = EventTypeSize.Half,
                    Icon = "bi-piggy-bank",
                    ColorClass = "text-success"
                },
                new EventType 
                { 
                    Id = 3, 
                    Name = "Student/Family/Community Support", 
                    Slug = "community-events",
                    Description = "Events designed to build community, support families, and create fun experiences for students.", 
                    IsMandatory = false,
                    DisplayOrder = 3,
                    IsActive = true,
                    Size = EventTypeSize.Half,
                    Icon = "bi-people",
                    ColorClass = "text-primary"
                },
                new EventType 
                { 
                    Id = 4, 
                    Name = "School/Teacher/Staff Support", 
                    Slug = "school-support-events",
					Description = "Events focused on supporting our educators, staff, and school operations.", 
                    IsMandatory = false,
                    DisplayOrder = 4,
                    IsActive = true,
                    Size = EventTypeSize.Half,
                    Icon = "bi-mortarboard",
                    ColorClass = "text-info"
                }
            );

            // Seed SchoolYears
            builder.Entity<SchoolYear>().HasData(
                new SchoolYear { Id = 1, Name = "2024-2025", StartDate = new DateTime(2024, 7, 1), EndDate = new DateTime(2025, 6, 30) },
                new SchoolYear { Id = 2, Name = "2025-2026", StartDate = new DateTime(2025, 7, 1), EndDate = new DateTime(2026, 6, 30) }
            );

            // Configure EventTypeSize enum to be stored as integer
            builder.Entity<EventType>()
                .Property(e => e.Size)
                .HasConversion<int>();
        }
    }
}
