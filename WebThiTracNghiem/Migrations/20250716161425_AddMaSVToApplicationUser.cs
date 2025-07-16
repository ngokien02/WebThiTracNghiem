using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class AddMaSVToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaSV",
                table: "AspNetUsers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaSV",
                table: "AspNetUsers");
        }
    }
}
