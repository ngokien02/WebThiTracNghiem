using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class AddTableKetQua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KetQua",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSinhVien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeThiId = table.Column<int>(type: "int", nullable: false),
                    SoCauDung = table.Column<int>(type: "int", nullable: true),
                    Diem = table.Column<double>(type: "float", nullable: true),
                    GioLam = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioNop = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQua", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetQua_AspNetUsers_IdSinhVien",
                        column: x => x.IdSinhVien,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KetQua_DeThi_DeThiId",
                        column: x => x.DeThiId,
                        principalTable: "DeThi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KetQua_DeThiId",
                table: "KetQua",
                column: "DeThiId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQua_IdSinhVien",
                table: "KetQua",
                column: "IdSinhVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KetQua");
        }
    }
}
