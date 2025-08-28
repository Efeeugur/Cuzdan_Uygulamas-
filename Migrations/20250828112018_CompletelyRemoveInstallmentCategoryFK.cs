using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cüzdan_Uygulaması.Migrations
{
    /// <inheritdoc />
    public partial class CompletelyRemoveInstallmentCategoryFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Categories_CategoryId",
                table: "Installments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
