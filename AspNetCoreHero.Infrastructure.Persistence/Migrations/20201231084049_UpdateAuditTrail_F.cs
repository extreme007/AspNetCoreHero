using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetCoreHero.Infrastructure.Persistence.Migrations
{
    public partial class UpdateAuditTrail_F : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ProductCategory");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ProductCategory");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Products",
                newName: "LastModifiedOn");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Products",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "ProductCategory",
                newName: "LastModifiedOn");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "ProductCategory",
                newName: "CreatedOn");

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "LastModifiedOn",
                table: "Products",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Products",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "LastModifiedOn",
                table: "ProductCategory",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "ProductCategory",
                newName: "Created");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ProductCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ProductCategory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });
        }
    }
}
