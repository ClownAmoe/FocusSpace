using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FocusSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Planets_PlanetId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Planets_PlanetId",
                table: "Tasks",
                column: "PlanetId",
                principalTable: "Planets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Planets_PlanetId",
                table: "Tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Planets_PlanetId",
                table: "Tasks",
                column: "PlanetId",
                principalTable: "Planets",
                principalColumn: "Id");
        }
    }
}
