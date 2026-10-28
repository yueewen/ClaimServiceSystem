using ClaimServiceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimServiceSystem.Infrastructure.Persistence;

public sealed class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Claimant> Claimants => Set<Claimant>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<ClaimAssessment> ClaimAssessments => Set<ClaimAssessment>();
    public DbSet<ClaimDecision> ClaimDecisions => Set<ClaimDecision>();
    public DbSet<InformationRequest> InformationRequests => Set<InformationRequest>();
    public DbSet<InformationRequestResponse> InformationRequestResponses => Set<InformationRequestResponse>();
    public DbSet<Settlement> Settlements => Set<Settlement>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistoryEntries => Set<ClaimStatusHistory>();
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claimant>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalUserId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(x => x.ExternalUserId).IsUnique();
        });

        modelBuilder.Entity<StaffUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalUserId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Role).HasConversion<string>().IsRequired().HasMaxLength(20);
            entity.HasIndex(x => x.ExternalUserId).IsUnique();
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Claimant)
                .WithMany(x => x.Claims)
                .HasForeignKey(x => x.ClaimantEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedOfficer)
                .WithMany(x => x.AssignedClaims)
                .HasForeignKey(x => x.AssignedOfficerEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClaimantEntityId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.ClaimantEntityId, x.SubmittedAtUtc });

            entity.Property(x => x.ClaimantEntityId).IsRequired();
            entity.Property(x => x.PolicyNumber).IsRequired().HasMaxLength(100);
            entity.Property(x => x.ClaimType).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Status).HasConversion<string>().IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<ClaimAssessment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssessedAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Notes).IsRequired().HasMaxLength(2000);

            entity.HasOne(x => x.Claim)
                .WithMany(x => x.Assessments)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RecordedByStaff)
                .WithMany(x => x.RecordedAssessments)
                .HasForeignKey(x => x.RecordedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClaimId).IsUnique();
        });

        modelBuilder.Entity<ClaimDecision>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Decision).HasConversion<string>().IsRequired().HasMaxLength(20);
            entity.Property(x => x.Reason).IsRequired().HasMaxLength(500);

            entity.HasOne(x => x.Claim)
                .WithOne(x => x.ClaimDecision)
                .HasForeignKey<ClaimDecision>(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.DecidedByStaff)
                .WithMany(x => x.RecordedDecisions)
                .HasForeignKey(x => x.DecidedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClaimId).IsUnique();
        });

        modelBuilder.Entity<InformationRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.Status).HasConversion<string>().IsRequired().HasMaxLength(20);

            entity.HasOne(x => x.Claim)
                .WithMany(x => x.InformationRequests)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RequestedByStaff)
                .WithMany(x => x.RequestedInformationItems)
                .HasForeignKey(x => x.RequestedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ClaimId, x.Status });
        });

        modelBuilder.Entity<InformationRequestResponse>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ResponseText).IsRequired().HasMaxLength(2000);

            entity.HasOne(x => x.InformationRequest)
                .WithMany(x => x.Responses)
                .HasForeignKey(x => x.InformationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RespondedByClaimant)
                .WithMany()
                .HasForeignKey(x => x.RespondedByClaimantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.State).HasConversion<string>().IsRequired().HasMaxLength(30);

            entity.HasOne(x => x.Claim)
                .WithOne(x => x.Settlement)
                .HasForeignKey<Settlement>(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CreatedByStaff)
                .WithMany(x => x.CreatedSettlements)
                .HasForeignKey(x => x.CreatedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ClaimId).IsUnique();
        });

        modelBuilder.Entity<ClaimStatusHistory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromStatus).HasConversion<string>().IsRequired().HasMaxLength(50);
            entity.Property(x => x.ToStatus).HasConversion<string>().IsRequired().HasMaxLength(50);
            entity.Property(x => x.ChangedByUserId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Reason).HasMaxLength(500);

            entity.HasOne(x => x.Claim)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.ClaimId);
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(100);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(50);
            entity.Property(x => x.SubjectId).IsRequired().HasMaxLength(100);

            entity.HasIndex(x => x.Username).IsUnique();
        });
    }
}   
