using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiedraAzul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DoctorAvailabilitySlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "DoctorAvailabilitySlots",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilitySlots_DoctorId_IsDeleted",
                table: "DoctorAvailabilitySlots",
                columns: new[] { "DoctorId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilitySlots_DoctorId_IsDeleted",
                table: "DoctorAvailabilitySlots");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DoctorAvailabilitySlots");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "DoctorAvailabilitySlots");
        }
    }
}
