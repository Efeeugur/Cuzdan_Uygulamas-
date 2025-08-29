using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cüzdan_Uygulaması.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceTransactionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecurrenceRule",
                table: "Transactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrenceRule",
                table: "Transactions");
        }
    }
}
