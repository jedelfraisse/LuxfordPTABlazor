using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Enums;	

namespace LuxfordPTAWeb.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
	public DbSet<Event> Events { get; set; }
	public DbSet<Sponsor> Sponsors { get; set; }
	public DbSet<EventMainSponsor> EventMainSponsors { get; set; }
	public DbSet<EventOtherSponsor> EventOtherSponsors { get; set; }
	public DbSet<SchoolYear> SchoolYears { get; set; }
	public DbSet<EventCat> EventCats { get; set; }
	public DbSet<SponsorAssignment> SponsorAssignments { get; set; }
	public DbSet<EventCatSub> EventCatSubs { get; set; }

	public DbSet<BoardPositionTitle> BoardPositionTitles { get; set; }
	public DbSet<BoardPosition> BoardPositions { get; set; }

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

		// EventCat enum configurations
		builder.Entity<EventCat>()
			.Property(e => e.Size)
			.HasConversion<int>();

		builder.Entity<EventCat>()
			.Property(e => e.DisplayMode)
			.HasConversion<int>();

		builder.Entity<BoardPosition>()
			.HasOne(bp => bp.SchoolYear)
			.WithMany(sy => sy.BoardPositions)
			.HasForeignKey(bp => bp.SchoolYearId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<BoardPosition>()
			.HasOne(bp => bp.AssignedUser)
			.WithMany()
			.HasForeignKey(bp => bp.UserId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.Entity<BoardPositionTitle>()
			.Property(bpt => bpt.RoleType)
			.HasConversion<int>();

		builder.Entity<BoardPosition>()
			.HasOne(bp => bp.BoardPositionTitle)
			.WithMany(bpt => bpt.BoardPositions)
			.HasForeignKey(bp => bp.BoardPositionTitleId)
			.OnDelete(DeleteBehavior.Restrict);
	}

	public static async Task SeedBoardPositionTitlesAsync(ApplicationDbContext db)
	{
		var titles = new[]
		{
		new BoardPositionTitle
		{
			Title = "President",
			RoleType = BoardRoleType.Officer,
			SortOrder = 1,
			IsRequired = true,
			Description = "PTA President",
			IsElected = true
		},
		new BoardPositionTitle
		{
			Title = "Vice President",
			RoleType = BoardRoleType.Officer,
			SortOrder = 2,
			IsRequired = true,
			Description = "PTA Vice President",
			IsElected = true
		},
		new BoardPositionTitle
		{
			Title = "Treasurer",
			RoleType = BoardRoleType.Officer,
			SortOrder = 3,
			IsRequired = true,
			Description = "PTA Treasurer",
			IsElected = true
		},
		new BoardPositionTitle
		{
			Title = "Secretary",
			RoleType = BoardRoleType.Officer,
			SortOrder = 4,
			IsRequired = true,
			Description = "PTA Secretary",
			IsElected = true
		},
		new BoardPositionTitle
		{
			Title = "Principal",
			RoleType = BoardRoleType.ExOfficio,
			SortOrder = 5,
			IsRequired = true,
			Description = "School Principal",
			IsElected = false
		},
		new BoardPositionTitle
		{
			Title = "VP of Volunteer Engagement",
			RoleType = BoardRoleType.CommitteeChair,
			SortOrder = 6,
			IsRequired = false,
			Description = "Vice President of Volunteer Engagement",
			IsElected = false
		}
	};

		foreach (var title in titles)
		{
			if (!db.BoardPositionTitles.Any(t => t.Title == title.Title))
			{
				db.BoardPositionTitles.Add(title);
			}
		}

		await db.SaveChangesAsync();
	}
}
