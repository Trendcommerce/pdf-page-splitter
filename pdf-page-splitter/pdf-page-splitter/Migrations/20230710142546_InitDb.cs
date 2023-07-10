using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace pdf_page_splitter.Migrations
{
    public partial class InitDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    FailedReason = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SplittedFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SlaName = table.Column<string>(type: "TEXT", nullable: true),
                    PageType = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    FailedReason = table.Column<string>(type: "TEXT", nullable: true),
                    PageNumbersInBasePdf = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    UploadedFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplittedFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SplittedFile_UploadedFile_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SplittedFile_UploadedFileId",
                table: "SplittedFile",
                column: "UploadedFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SplittedFile");

            migrationBuilder.DropTable(
                name: "UploadedFile");
        }
    }
}
