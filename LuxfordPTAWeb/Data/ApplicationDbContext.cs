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

			// Seed EventTypes
			builder.Entity<EventType>().HasData(
				new EventType { Id = 1, Name = "School Closed & Special Days", IsMandatory = true },
				new EventType { Id = 2, Name = "Fundraising Events", IsMandatory = false },
				new EventType { Id = 3, Name = "Student/Family/Community Support", IsMandatory = false },
				new EventType { Id = 4, Name = "School/Teacher/Staff Support", IsMandatory = false }
			);

			// Seed a SchoolYear example
			builder.Entity<SchoolYear>().HasData(
				new SchoolYear { Id = 1, Name = "2024-2025", StartDate = new DateTime(2024, 7, 1), EndDate = new DateTime(2025, 6, 30) },
				new SchoolYear { Id = 2, Name = "2025-2026", StartDate = new DateTime(2025, 7, 1), EndDate = new DateTime(2026, 6, 30) }

			);
		}
	}
}
