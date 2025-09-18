using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class updateModelKQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietKQ_DapAn_DapAnId",
                table: "ChiTietKQ");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietKQ_DapAnId",
                table: "ChiTietKQ");

            migrationBuilder.DropColumn(
                name: "DapAnId",
                table: "ChiTietKQ");

            migrationBuilder.AlterColumn<double>(
                name: "Diem",
                table: "ChiTietKQ",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateTable(
                name: "ChiTietKQ_DapAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChiTietKQId = table.Column<int>(type: "int", nullable: false),
                    DapAnId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKQ_DapAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietKQ_DapAn_ChiTietKQ_ChiTietKQId",
                        column: x => x.ChiTietKQId,
                        principalTable: "ChiTietKQ",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietKQ_DapAn_DapAn_DapAnId",
                        column: x => x.DapAnId,
                        principalTable: "DapAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKQ_DapAn_ChiTietKQId",
                table: "ChiTietKQ_DapAn",
                column: "ChiTietKQId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKQ_DapAn_DapAnId",
                table: "ChiTietKQ_DapAn",
                column: "DapAnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietKQ_DapAn");

            migrationBuilder.AlterColumn<double>(
                name: "Diem",
                table: "ChiTietKQ",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DapAnId",
                table: "ChiTietKQ",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKQ_DapAnId",
                table: "ChiTietKQ",
                column: "DapAnId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietKQ_DapAn_DapAnId",
                table: "ChiTietKQ",
                column: "DapAnId",
                principalTable: "DapAn",
                principalColumn: "Id");
        }
    }
}
