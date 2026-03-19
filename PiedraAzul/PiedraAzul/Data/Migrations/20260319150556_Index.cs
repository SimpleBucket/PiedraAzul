using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiedraAzul.Migrations
{
    /// <inheritdoc />
    public partial class Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorAvailabilitySlots_DoctorAvailabilityBloc~",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorProfiles_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorAvailabilityBlockId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorAvailabilityBlockId_DayOfYear",
                table: "Appointments",
                columns: new[] { "DoctorAvailabilityBlockId", "DayOfYear" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorAvailabilitySlots_DoctorAvailabilityBloc~",
                table: "Appointments",
                column: "DoctorAvailabilityBlockId",
                principalTable: "DoctorAvailabilitySlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorProfiles_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "PatientProfiles",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorAvailabilitySlots_DoctorAvailabilityBloc~",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorProfiles_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorAvailabilityBlockId_DayOfYear",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorAvailabilityBlockId",
                table: "Appointments",
                column: "DoctorAvailabilityBlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorAvailabilitySlots_DoctorAvailabilityBloc~",
                table: "Appointments",
                column: "DoctorAvailabilityBlockId",
                principalTable: "DoctorAvailabilitySlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorProfiles_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "DoctorProfiles",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PatientProfiles_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "PatientProfiles",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
