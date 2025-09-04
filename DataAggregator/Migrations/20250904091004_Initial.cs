using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAggregator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:pharmacy_site_module", "apteka103by,tabletka_by");

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NameOriginal = table.Column<string>(type: "text", nullable: false),
                    NameTranslate = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<string>(type: "text", nullable: false),
                    FormOriginal = table.Column<string>(type: "text", nullable: false),
                    FormTranslate = table.Column<string>(type: "text", nullable: false),
                    ManufacturerOriginal = table.Column<string>(type: "text", nullable: false),
                    ManufacturerTranslate = table.Column<string>(type: "text", nullable: false),
                    CountryOriginal = table.Column<string>(type: "text", nullable: false),
                    CountryTranslate = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sites_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_Id",
                table: "Drugs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_NameOriginal",
                table: "Drugs",
                column: "NameOriginal");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_DrugId",
                table: "Sites",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Id",
                table: "Sites",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sites");

            migrationBuilder.DropTable(
                name: "Drugs");
        }
    }
}
