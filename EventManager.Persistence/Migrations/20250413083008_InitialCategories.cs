using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("013c2392-03d2-cedf-6fa2-bd51e0cfd2cb"), "Weddings" },
                    { new Guid("124d34a3-14e3-dfe0-70b3-ce62f1d0e3dc"), "Charity" },
                    { new Guid("7e345e1a-9b5a-4c5e-8d2a-3e7f4b5c6d7e"), "Conferences" },
                    { new Guid("8f456f2b-9c6b-5d6f-9e3b-4f8a5c6d7e8f"), "Workshops" },
                    { new Guid("9a567f3c-ad7c-6e7f-0f4c-5d9b8a7c6d5e"), "Festivals" },
                    { new Guid("bcd89e4d-be8d-7f8a-1e5d-6e0c9b8a7f6e"), "Meetups" },
                    { new Guid("cde9af5e-cf9e-8a9b-2b6e-7f1dac9b8e7f"), "Exhibitions" },
                    { new Guid("def0b06f-d0af-9bac-3c7f-8a2ebdac9f8a"), "Concerts" },
                    { new Guid("ef1a0170-e1b0-acbd-4d80-9b3fcebda0a9"), "Sports" },
                    { new Guid("f02b1281-f2c1-bdce-5e91-ac40dfceb1ba"), "Seminar" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("013c2392-03d2-cedf-6fa2-bd51e0cfd2cb"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("124d34a3-14e3-dfe0-70b3-ce62f1d0e3dc"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7e345e1a-9b5a-4c5e-8d2a-3e7f4b5c6d7e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8f456f2b-9c6b-5d6f-9e3b-4f8a5c6d7e8f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("9a567f3c-ad7c-6e7f-0f4c-5d9b8a7c6d5e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("bcd89e4d-be8d-7f8a-1e5d-6e0c9b8a7f6e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cde9af5e-cf9e-8a9b-2b6e-7f1dac9b8e7f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("def0b06f-d0af-9bac-3c7f-8a2ebdac9f8a"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ef1a0170-e1b0-acbd-4d80-9b3fcebda0a9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f02b1281-f2c1-bdce-5e91-ac40dfceb1ba"));
        }
    }
}
