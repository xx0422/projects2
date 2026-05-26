using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAppointment.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Speciality",
                table: "AspNetUsers",
                newName: "Specialty");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Specialty",
                table: "AspNetUsers",
                newName: "Speciality");
        }
    }
}
