using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FocusSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlanetsAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Planets",
                columns: new[] { "Id", "Description", "DistanceFromPrevious", "ImageUrl", "Name", "OrderNumber" },
                values: new object[,]
                {
                    { 1, "The closest planet to the Sun", null, null, "Mercury", 1 },
                    { 2, "The hottest planet", null, null, "Venus", 2 },
                    { 3, "Our home planet", null, null, "Earth", 3 },
                    { 4, "The Red Planet", null, null, "Mars", 4 },
                    { 5, "The largest planet", null, null, "Jupiter", 5 },
                    { 6, "The ringed planet", null, null, "Saturn", 6 },
                    { 7, "The ice giant", null, null, "Uranus", 7 },
                    { 8, "The farthest planet", null, null, "Neptune", 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Planets",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
