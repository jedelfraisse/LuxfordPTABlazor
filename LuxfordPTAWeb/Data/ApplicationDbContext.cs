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

            // Configure EventTypeSize enum to be stored as integer
            builder.Entity<EventType>()
                .Property(e => e.Size)
                .HasConversion<int>();

            // Configure EventDisplayMode enum to be stored as integer
            builder.Entity<EventType>()
                .Property(e => e.DisplayMode)
                .HasConversion<int>();
        }
    }
}
