using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class AddPostImgUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "DeMuc",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "DeMuc");
        }
    }
}
