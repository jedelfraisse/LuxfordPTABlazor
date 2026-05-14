using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Enums;	

namespace LuxfordPTAWeb.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
	public DbSet<Event> Events { get; set; }
	public DbSet<EventDay> EventDays { get; set; }
	public DbSet<Sponsor> Sponsors { get; set; }
	public DbSet<EventMainSponsor> EventMainSponsors { get; set; }
	public DbSet<EventOtherSponsor> EventOtherSponsors { get; set; }
	public DbSet<SchoolYear> SchoolYears { get; set; }
	public DbSet<EventCat> EventCats { get; set; }
	public DbSet<SponsorAssignment> SponsorAssignments { get; set; }
	public DbSet<EventCatSub> EventCatSubs { get; set; }
	public DbSet<EventTemplate> EventTemplates { get; set; }
	public DbSet<EventControl> EventControls { get; set; }
	public DbSet<TalentShowSessionState> TalentShowSessionStates { get; set; }
	public DbSet<TalentShowAct> TalentShowActs { get; set; }
	public DbSet<TalentShowSignup> TalentShowSignups { get; set; }
	public DbSet<TalentShowTryOutEntry> TalentShowTryOutEntries { get; set; }
	public DbSet<TalentShowVote> TalentShowVotes { get; set; }
	public DbSet<TalentShowDisplayAssignmentHistory> TalentShowDisplayAssignmentHistories { get; set; }
	public DbSet<ProgramCard> ProgramCards { get; set; }

	// Summit Proposal: Bug Reports
	public DbSet<BugReport> BugReports { get; set; }

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

		// Event copy/source relationship configuration
		builder.Entity<Event>()
			.HasOne(e => e.SourceEvent)
			.WithMany(e => e.CopiedEvents)
			.HasForeignKey(e => e.SourceEventId)
			.OnDelete(DeleteBehavior.Restrict)
			.HasConstraintName("FK_Events_SourceEvent");

		// Event approval relationship configuration
		builder.Entity<Event>()
			.HasOne(e => e.ApprovedBy)
			.WithMany()
			.HasForeignKey(e => e.ApprovedByUserId)
			.OnDelete(DeleteBehavior.NoAction)
			.HasConstraintName("FK_Events_ApprovedBy");

		// EventDay relationship configuration
		builder.Entity<EventDay>()
			.HasOne(ed => ed.Event)
			.WithMany(e => e.EventDays)
			.HasForeignKey(ed => ed.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		// Event controls relationship configuration
		builder.Entity<EventControl>()
			.HasOne(ec => ec.Event)
			.WithMany(e => e.EventControls)
			.HasForeignKey(ec => ec.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<EventControl>()
			.Property(ec => ec.ControlType)
			.HasMaxLength(100);

		builder.Entity<EventControl>()
			.Property(ec => ec.DisplayName)
			.HasMaxLength(120);

		builder.Entity<EventControl>()
			.HasIndex(ec => new { ec.EventId, ec.IsDirector })
			.HasFilter("[IsDirector] = 1")
			.IsUnique();

		builder.Entity<EventControl>()
			.HasIndex(ec => new { ec.EventId, ec.SequenceOrder });

		builder.Entity<TalentShowSessionState>()
			.HasOne(ts => ts.Event)
			.WithOne(e => e.TalentShowSessionState)
			.HasForeignKey<TalentShowSessionState>(ts => ts.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<TalentShowSessionState>()
			.Property(ts => ts.CurrentState)
			.HasMaxLength(40);

		builder.Entity<TalentShowSessionState>()
			.Property(ts => ts.SessionCode)
			.HasMaxLength(24);

		builder.Entity<TalentShowSessionState>()
			.HasIndex(ts => ts.EventId)
			.IsUnique();

		builder.Entity<TalentShowSessionState>()
			.HasIndex(ts => ts.SessionCode);

		builder.Entity<TalentShowAct>()
			.HasOne(ta => ta.Event)
			.WithMany(e => e.TalentShowActs)
			.HasForeignKey(ta => ta.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<TalentShowAct>()
			.Property(ta => ta.PerformerName)
			.HasMaxLength(120);

		builder.Entity<TalentShowAct>()
			.Property(ta => ta.Title)
			.HasMaxLength(120);

		builder.Entity<TalentShowAct>()
			.HasIndex(ta => new { ta.EventId, ta.OrderIndex })
			.IsUnique();

		builder.Entity<TalentShowSignup>()
			.HasOne(ts => ts.Event)
			.WithMany(e => e.TalentShowSignups)
			.HasForeignKey(ts => ts.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<TalentShowSignup>()
			.Property(ts => ts.PerformerNames)
			.HasMaxLength(240);

		builder.Entity<TalentShowSignup>()
			.Property(ts => ts.ActTitle)
			.HasMaxLength(120);

		builder.Entity<TalentShowSignup>()
			.Property(ts => ts.ContactEmail)
			.HasMaxLength(254);

		builder.Entity<TalentShowSignup>()
			.Property(ts => ts.Status)
			.HasMaxLength(40);

		builder.Entity<TalentShowSignup>()
			.HasIndex(ts => new { ts.EventId, ts.Status, ts.CreatedAtUtc });

		builder.Entity<TalentShowTryOutEntry>()
			.HasOne(te => te.Event)
			.WithMany(e => e.TalentShowTryOutEntries)
			.HasForeignKey(te => te.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<TalentShowTryOutEntry>()
			.HasOne(te => te.Signup)
			.WithMany()
			.HasForeignKey(te => te.SignupId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.Entity<TalentShowTryOutEntry>()
			.Property(te => te.PerformerNames)
			.HasMaxLength(240);

		builder.Entity<TalentShowTryOutEntry>()
			.Property(te => te.ActTitle)
			.HasMaxLength(120);

		builder.Entity<TalentShowTryOutEntry>()
			.Property(te => te.Status)
			.HasMaxLength(40);

		builder.Entity<TalentShowTryOutEntry>()
			.HasIndex(te => new { te.EventId, te.SlotTime });

		builder.Entity<TalentShowVote>()
			.HasOne(tv => tv.Event)
			.WithMany(e => e.TalentShowVotes)
			.HasForeignKey(tv => tv.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<TalentShowVote>()
			.HasOne(tv => tv.Act)
			.WithMany()
			.HasForeignKey(tv => tv.ActId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.Entity<TalentShowVote>()
			.HasIndex(tv => new { tv.EventId, tv.TimestampUtc });

		builder.Entity<TalentShowDisplayAssignmentHistory>()
			.HasOne(th => th.Event)
			.WithMany(e => e.TalentShowDisplayAssignments)
			.HasForeignKey(th => th.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<TalentShowDisplayAssignmentHistory>()
			.Property(th => th.DisplayCode)
			.HasMaxLength(16);

		builder.Entity<TalentShowDisplayAssignmentHistory>()
			.Property(th => th.Role)
			.HasMaxLength(40);

		builder.Entity<TalentShowDisplayAssignmentHistory>()
			.HasIndex(th => new { th.EventId, th.AssignedAt });

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

		// NEW: EventCat permission and coordinator requirement enum configurations
		builder.Entity<EventCat>()
			.Property(e => e.EditingPermission)
			.HasConversion<int>();

		builder.Entity<EventCat>()
			.Property(e => e.CoordinatorRequirement)
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

		// Indexes for better performance
		builder.Entity<Event>()
			.HasIndex(e => e.Slug);

		builder.Entity<Event>()
			.HasIndex(e => new { e.SchoolYearId, e.Status });

		builder.Entity<EventDay>()
			.HasIndex(ed => new { ed.EventId, ed.DayNumber })
			.IsUnique();

		builder.Entity<EventTemplate>()
			.HasOne(t => t.EventCat)
			.WithMany()
			.HasForeignKey(t => t.EventCatId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<EventTemplate>()
			.HasOne(t => t.EventCatSub)
			.WithMany()
			.HasForeignKey(t => t.EventSubTypeId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.Entity<EventTemplate>()
			.HasOne(t => t.SourceEvent)
			.WithMany()
			.HasForeignKey(t => t.SourceEventId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<EventTemplate>()
			.HasIndex(t => new { t.IsActive, t.EventCatId });

		builder.Entity<EventTemplate>()
			.HasIndex(t => t.SourceEventId);
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
