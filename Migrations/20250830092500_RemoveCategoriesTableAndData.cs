using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cüzdan_Uygulaması.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoriesTableAndData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First drop any foreign key constraints to Categories table (if they exist)
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
                               WHERE constraint_name = 'FK_Transactions_Categories_CategoryId' 
                               AND table_name = 'Transactions') THEN
                        ALTER TABLE ""Transactions"" DROP CONSTRAINT ""FK_Transactions_Categories_CategoryId"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
                               WHERE constraint_name = 'FK_Installments_Categories_CategoryId' 
                               AND table_name = 'Installments') THEN
                        ALTER TABLE ""Installments"" DROP CONSTRAINT ""FK_Installments_Categories_CategoryId"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
                               WHERE constraint_name = 'FK_Categories_AspNetUsers_UserId' 
                               AND table_name = 'Categories') THEN
                        ALTER TABLE ""Categories"" DROP CONSTRAINT ""FK_Categories_AspNetUsers_UserId"";
                    END IF;
                END $$;
            ");
            
            // Drop indexes on CategoryId columns if they exist
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Transactions_CategoryId"";
                DROP INDEX IF EXISTS ""IX_Installments_CategoryId"";
                DROP INDEX IF EXISTS ""IX_Categories_UserId"";
            ");
            
            // Finally drop the Categories table completely
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Categories\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration is not reversible as we're cleaning up unused data
            // The Categories table should not be recreated
            throw new NotSupportedException("This migration removes unused Categories table and cannot be reversed.");
        }
    }
}