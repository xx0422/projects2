using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalAppointment.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Speciality",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Speciality",
                table: "AspNetUsers");
        }
    }
}
