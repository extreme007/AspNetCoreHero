using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetCoreHero.Infrastructure.Persistence.Migrations
{
    public partial class propertyActivityLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentValue",
                table: "ActivityLogs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalValue",
                table: "ActivityLogs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "OriginalValue",
                table: "ActivityLogs");
        }
    }
}
