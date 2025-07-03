using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class AddRandomProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RandomCauHoi",
                table: "DeThi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RandomDapAn",
                table: "DeThi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowKQ",
                table: "DeThi",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RandomCauHoi",
                table: "DeThi");

            migrationBuilder.DropColumn(
                name: "RandomDapAn",
                table: "DeThi");

            migrationBuilder.DropColumn(
                name: "ShowKQ",
                table: "DeThi");
        }
    }
}
