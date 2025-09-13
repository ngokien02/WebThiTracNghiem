using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHoi_DeThi_DeThiId",
                table: "CauHoi");

            migrationBuilder.AlterColumn<int>(
                name: "DeThiId",
                table: "CauHoi",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ChuDeId",
                table: "CauHoi",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChiTietDeThi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeThiId = table.Column<int>(type: "int", nullable: false),
                    CauHoiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDeThi_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDeThi_DeThi_DeThiId",
                        column: x => x.DeThiId,
                        principalTable: "DeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietKQ",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KetQuaId = table.Column<int>(type: "int", nullable: false),
                    CauHoiId = table.Column<int>(type: "int", nullable: false),
                    DapAnId = table.Column<int>(type: "int", nullable: true),
                    Diem = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietKQ", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietKQ_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietKQ_DapAn_DapAnId",
                        column: x => x.DapAnId,
                        principalTable: "DapAn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChiTietKQ_KetQua_KetQuaId",
                        column: x => x.KetQuaId,
                        principalTable: "KetQua",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuDe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCD = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuDe", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_ChuDeId",
                table: "CauHoi",
                column: "ChuDeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDeThi_CauHoiId",
                table: "ChiTietDeThi",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDeThi_DeThiId",
                table: "ChiTietDeThi",
                column: "DeThiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKQ_CauHoiId",
                table: "ChiTietKQ",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKQ_DapAnId",
                table: "ChiTietKQ",
                column: "DapAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietKQ_KetQuaId",
                table: "ChiTietKQ",
                column: "KetQuaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoi_ChuDe_ChuDeId",
                table: "CauHoi",
                column: "ChuDeId",
                principalTable: "ChuDe",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoi_DeThi_DeThiId",
                table: "CauHoi",
                column: "DeThiId",
                principalTable: "DeThi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHoi_ChuDe_ChuDeId",
                table: "CauHoi");

            migrationBuilder.DropForeignKey(
                name: "FK_CauHoi_DeThi_DeThiId",
                table: "CauHoi");

            migrationBuilder.DropTable(
                name: "ChiTietDeThi");

            migrationBuilder.DropTable(
                name: "ChiTietKQ");

            migrationBuilder.DropTable(
                name: "ChuDe");

            migrationBuilder.DropIndex(
                name: "IX_CauHoi_ChuDeId",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "ChuDeId",
                table: "CauHoi");

            migrationBuilder.AlterColumn<int>(
                name: "DeThiId",
                table: "CauHoi",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CauHoi_DeThi_DeThiId",
                table: "CauHoi",
                column: "DeThiId",
                principalTable: "DeThi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
