using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketBox.Migrations
{
    /// <inheritdoc />
    public partial class FieldRenameInTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Seat",
                table: "Tickets",
                newName: "Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Tickets",
                newName: "Seat");
        }
    }
}
