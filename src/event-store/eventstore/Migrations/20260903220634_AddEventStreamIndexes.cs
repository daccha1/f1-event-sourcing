using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eventstore.Migrations
{
    /// <inheritdoc />
    public partial class AddEventStreamIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RootId",
                table: "Events",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "Events",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventType",
                table: "Events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RootId",
                table: "Events",
                column: "RootId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_EventType",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_RootId",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "RootId",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
