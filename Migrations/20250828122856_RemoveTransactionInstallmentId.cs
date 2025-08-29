using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cüzdan_Uygulaması.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTransactionInstallmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Installments_InstallmentId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Installments_InstallmentId",
                table: "Transactions",
                column: "InstallmentId",
                principalTable: "Installments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Installments_InstallmentId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Installments_InstallmentId",
                table: "Transactions",
                column: "InstallmentId",
                principalTable: "Installments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
