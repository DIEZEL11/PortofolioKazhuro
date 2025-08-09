using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortofolioKazhuro.Migrations
{
    /// <inheritdoc />
    public partial class AddBot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramChatIdBot",
                table: "Profiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramTokenBot",
                table: "Profiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramChatIdBot",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "TelegramTokenBot",
                table: "Profiles");
        }
    }
}
