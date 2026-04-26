using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lending.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LenderIdIndexOnLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_loans_one_time_due_date_after_created",
                schema: "lending",
                table: "loans");

            migrationBuilder.CreateIndex(
                name: "IX_loans_lender_id",
                schema: "lending",
                table: "loans",
                column: "lender_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_loans_one_time_due_date_after_created",
                schema: "lending",
                table: "loans",
                sql: "one_time_due_date IS NULL OR one_time_due_date >= (created_at AT TIME ZONE 'America/Santo_Domingo')::date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loans_lender_id",
                schema: "lending",
                table: "loans");

            migrationBuilder.DropCheckConstraint(
                name: "ck_loans_one_time_due_date_after_created",
                schema: "lending",
                table: "loans");

            migrationBuilder.AddCheckConstraint(
                name: "ck_loans_one_time_due_date_after_created",
                schema: "lending",
                table: "loans",
                sql: "one_time_due_date IS NULL OR one_time_due_date >= created_at");
        }
    }
}
