using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DiemToiDa",
                table: "DeThi",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemToiDa",
                table: "DeThi");
        }
    }
}
