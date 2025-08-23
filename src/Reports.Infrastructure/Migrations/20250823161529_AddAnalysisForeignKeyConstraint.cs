using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reports.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // COMMENTED OUT: Cross-database foreign key constraints don't work in Docker containers
            // where each microservice has its own isolated database instance.
            // Instead, we implement application-level validation through HTTP service communication.

            // ORIGINAL CODE (commented for Docker compatibility):
            // migrationBuilder.Sql(@"
            //     ALTER TABLE REPORTS 
            //     ADD CONSTRAINT fk_reports_analysis 
            //     FOREIGN KEY (analysis_id) REFERENCES analysisdb.ANALYSIS(id) 
            //     ON DELETE CASCADE;
            // ");

            // migrationBuilder.Sql(@"
            //     ALTER TABLE HISTORY 
            //     ADD CONSTRAINT fk_history_analysis 
            //     FOREIGN KEY (analysis_id) REFERENCES analysisdb.ANALYSIS(id) 
            //     ON DELETE CASCADE;
            // ");

            // NOTE: Data integrity is now enforced at the application level through:
            // 1. AnalysisValidationService - validates analysis_id exists via HTTP call to Analysis API
            // 2. Service-to-service communication for data consistency
            // 3. Application-level cascade operations if needed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // COMMENTED OUT: No foreign keys to drop since we're using application-level validation

            // ORIGINAL CODE (commented for Docker compatibility):
            // migrationBuilder.Sql(@"
            //     ALTER TABLE REPORTS 
            //     DROP FOREIGN KEY fk_reports_analysis;
            // ");

            // migrationBuilder.Sql(@"
            //     ALTER TABLE HISTORY 
            //     DROP FOREIGN KEY fk_history_analysis;
            // ");
        }
    }
}
