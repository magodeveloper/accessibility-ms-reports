using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reports.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // COMMENTED OUT: Cross-database foreign key constraints don't work in Docker containers
            // where each microservice has its own isolated database instance.
            // Instead, we implement application-level validation through HTTP service communication.

            // ORIGINAL CODE (commented for Docker compatibility):
            // migrationBuilder.Sql(@"
            //     ALTER TABLE HISTORY 
            //     ADD CONSTRAINT fk_history_user 
            //     FOREIGN KEY (user_id) REFERENCES usersdb.USERS(id) 
            //     ON DELETE CASCADE;
            // ");

            // NOTE: Data integrity is now enforced at the application level through:
            // 1. UserValidationService - validates user_id exists via HTTP call to Users API
            // 2. Service-to-service communication for data consistency
            // 3. Application-level cascade operations if needed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // COMMENTED OUT: No foreign key to drop since we're using application-level validation

            // ORIGINAL CODE (commented for Docker compatibility):
            // migrationBuilder.Sql(@"
            //     ALTER TABLE HISTORY 
            //     DROP FOREIGN KEY fk_history_user;
            // ");
        }
    }
}
