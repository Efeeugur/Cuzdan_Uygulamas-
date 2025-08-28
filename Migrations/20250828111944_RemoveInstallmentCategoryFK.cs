using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cüzdan_Uygulaması.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstallmentCategoryFK : Migration
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
            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Categories_CategoryId",
                table: "Installments");

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Categories_CategoryId",
                table: "Installments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
