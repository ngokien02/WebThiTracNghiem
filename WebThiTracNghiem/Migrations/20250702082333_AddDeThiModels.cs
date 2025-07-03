using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class AddDeThiModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeThi",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GioBD = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioKT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoCauHoi = table.Column<int>(type: "int", nullable: false),
                    IdGiangVien = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeThi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeThi_AspNetUsers_IdGiangVien",
                        column: x => x.IdGiangVien,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeThiId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHoi_DeThi_DeThiId",
                        column: x => x.DeThiId,
                        principalTable: "DeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DapAn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DungSai = table.Column<bool>(type: "bit", nullable: false),
                    CauHoiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DapAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DapAn_CauHoi_CauHoiId",
                        column: x => x.CauHoiId,
                        principalTable: "CauHoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_DeThiId",
                table: "CauHoi",
                column: "DeThiId");

            migrationBuilder.CreateIndex(
                name: "IX_DapAn_CauHoiId",
                table: "DapAn",
                column: "CauHoiId");

            migrationBuilder.CreateIndex(
                name: "IX_DeThi_IdGiangVien",
                table: "DeThi",
                column: "IdGiangVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DapAn");

            migrationBuilder.DropTable(
                name: "CauHoi");

            migrationBuilder.DropTable(
                name: "DeThi");
        }
    }
}
