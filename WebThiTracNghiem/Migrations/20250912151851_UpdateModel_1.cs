using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHoi_DeThi_DeThiId",
                table: "CauHoi");

            migrationBuilder.DropIndex(
                name: "IX_CauHoi_DeThiId",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "DeThiId",
                table: "CauHoi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeThiId",
                table: "CauHoi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_DeThiId",
                table: "CauHoi",
                column: "DeThiId");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoi_DeThi_DeThiId",
                table: "CauHoi",
                column: "DeThiId",
                principalTable: "DeThi",
                principalColumn: "Id");
        }
    }
}
