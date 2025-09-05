using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orchestrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaRecivedSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "DrugCollectionSagaState");

            migrationBuilder.DropColumn(
                name: "ResponseAddress",
                table: "DrugCollectionSagaState");

            migrationBuilder.AddColumn<int[]>(
                name: "ReceivedSources",
                table: "DrugCollectionSagaState",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedSources",
                table: "DrugCollectionSagaState");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "DrugCollectionSagaState",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseAddress",
                table: "DrugCollectionSagaState",
                type: "text",
                nullable: true);
        }
    }
}
