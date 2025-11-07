using Reports.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Reports.Infrastructure.Data;

public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options) { }

    public DbSet<Report> Reports => Set<Report>();
    public DbSet<History> History => Set<History>();

    public override int SaveChanges()
    {
        ConvertUtcToEcuadorTime();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertUtcToEcuadorTime();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ConvertUtcToEcuadorTime()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                ConvertPropertyToEcuadorTime(property);
            }
        }
    }

    private static void ConvertPropertyToEcuadorTime(PropertyEntry property)
    {
        if ((property.Metadata.ClrType == typeof(DateTime) ||
             property.Metadata.ClrType == typeof(DateTime?)) &&
            property.CurrentValue != null)
        {
            var dateTime = (DateTime)property.CurrentValue;
            // Si es UTC, restar 5 horas para Ecuador
            if (dateTime.Kind == DateTimeKind.Utc || dateTime.Kind == DateTimeKind.Local)
            {
                var ecuadorTime = dateTime.ToUniversalTime().AddHours(-5);
                property.CurrentValue = DateTime.SpecifyKind(ecuadorTime, DateTimeKind.Unspecified);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("REPORTS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnalysisId).HasColumnName("analysis_id");
            entity.Property(e => e.Format)
                .HasColumnName("format")
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<Reports.Domain.Entities.ReportFormat>(
                        v.Substring(0, 1).ToUpper() + v.Substring(1).ToLower()
                    )
                );
            entity.Property(e => e.FilePath).HasColumnName("file_path").IsRequired();
            entity.Property(e => e.GenerationDate).HasColumnName("generation_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
        modelBuilder.Entity<History>(entity =>
        {
            entity.ToTable("HISTORY");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AnalysisId).HasColumnName("analysis_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }
}