﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplyChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_Name",
                table: "Events");
        }
    }
}
