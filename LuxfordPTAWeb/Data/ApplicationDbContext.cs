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

            // Event Main Sponsor relationship configuration
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

            // Event Other Sponsor relationship configuration
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

            // Event Coordinator relationship configuration
            builder.Entity<Event>()
                .HasOne(e => e.EventCoordinator)
                .WithMany()
                .HasForeignKey(e => e.EventCoordinatorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Event Status enum configuration
            builder.Entity<Event>()
                .Property(e => e.Status)
                .HasConversion<int>();

            // EventType enum configurations
            builder.Entity<EventType>()
                .Property(e => e.Size)
                .HasConversion<int>();

            builder.Entity<EventType>()
                .Property(e => e.DisplayMode)
                .HasConversion<int>();
        }
    }
}
