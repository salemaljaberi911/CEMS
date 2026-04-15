using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CEMS.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceFlagToRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAttended",
                table: "Registrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAttended",
                table: "Registrations");
        }
    }
}
