using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Da3m.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "VisitReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "Prostheses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "PatientDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "ManufacturerDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "DonorDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "Donation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletd",
                table: "Doctor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Center",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "VisitReports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "Prostheses");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "PatientDetails");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "ManufacturerDetails");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "DonorDetails");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "Donation");

            migrationBuilder.DropColumn(
                name: "IsDeletd",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Center");
        }
    }
}
