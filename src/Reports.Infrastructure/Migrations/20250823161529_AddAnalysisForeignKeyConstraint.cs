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
            // Add cross-database foreign key constraint to reference analysisdb.ANALYSIS table
            migrationBuilder.Sql(@"
                ALTER TABLE REPORTS 
                ADD CONSTRAINT fk_reports_analysis 
                FOREIGN KEY (analysis_id) REFERENCES analysisdb.ANALYSIS(id) 
                ON DELETE CASCADE;
            ");

            // Add cross-database foreign key constraint for HISTORY table to reference analysisdb.ANALYSIS
            migrationBuilder.Sql(@"
                ALTER TABLE HISTORY 
                ADD CONSTRAINT fk_history_analysis 
                FOREIGN KEY (analysis_id) REFERENCES analysisdb.ANALYSIS(id) 
                ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the cross-database foreign key constraints
            migrationBuilder.Sql(@"
                ALTER TABLE REPORTS 
                DROP FOREIGN KEY fk_reports_analysis;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE HISTORY 
                DROP FOREIGN KEY fk_history_analysis;
            ");
        }
    }
}
