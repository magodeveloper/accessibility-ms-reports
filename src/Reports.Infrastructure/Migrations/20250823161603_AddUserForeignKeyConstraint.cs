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
            // Add cross-database foreign key constraint to reference usersdb.USERS table
            migrationBuilder.Sql(@"
                ALTER TABLE HISTORY 
                ADD CONSTRAINT fk_history_user 
                FOREIGN KEY (user_id) REFERENCES usersdb.USERS(id) 
                ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the cross-database foreign key constraint
            migrationBuilder.Sql(@"
                ALTER TABLE HISTORY 
                DROP FOREIGN KEY fk_history_user;
            ");
        }
    }
}
