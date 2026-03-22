using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiedraAzul.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PatientGuests_PatientGuestId",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "PatientGuestId",
                table: "Appointments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PatientGuests_PatientGuestId",
                table: "Appointments",
                column: "PatientGuestId",
                principalTable: "PatientGuests",
                principalColumn: "PatientIdentification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PatientGuests_PatientGuestId",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "PatientGuestId",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PatientGuests_PatientGuestId",
                table: "Appointments",
                column: "PatientGuestId",
                principalTable: "PatientGuests",
                principalColumn: "PatientIdentification",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
