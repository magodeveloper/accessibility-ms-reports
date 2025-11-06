using Reports.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Reports.Infrastructure.Data;

public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options) { }

    public DbSet<Report> Reports => Set<Report>();
    public DbSet<History> History => Set<History>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Convertidor de DateTime para forzar DateTimeKind.Local
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => DateTime.SpecifyKind(v, DateTimeKind.Local),
            v => DateTime.SpecifyKind(v, DateTimeKind.Local)
        );

        // Aplicar el convertidor a todas las propiedades DateTime
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }

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