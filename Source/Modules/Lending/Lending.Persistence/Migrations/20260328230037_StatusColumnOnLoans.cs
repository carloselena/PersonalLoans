using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lending.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StatusColumnOnLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OneTimeDueDate",
                schema: "lending",
                table: "loans",
                newName: "one_time_due_date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "one_time_due_date",
                schema: "lending",
                table: "loans",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "lending",
                table: "loans",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddCheckConstraint(
                name: "ck_loans_one_time_due_date_after_created",
                schema: "lending",
                table: "loans",
                sql: "one_time_due_date IS NULL OR one_time_due_date >= created_at");

            migrationBuilder.AddCheckConstraint(
                name: "ck_loans_status_valid",
                schema: "lending",
                table: "loans",
                sql: "status IN ('Draft','Active','PaidOff')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_loans_one_time_due_date_after_created",
                schema: "lending",
                table: "loans");

            migrationBuilder.DropCheckConstraint(
                name: "ck_loans_status_valid",
                schema: "lending",
                table: "loans");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "lending",
                table: "loans");

            migrationBuilder.RenameColumn(
                name: "one_time_due_date",
                schema: "lending",
                table: "loans",
                newName: "OneTimeDueDate");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OneTimeDueDate",
                schema: "lending",
                table: "loans",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 13);
        }
    }
}
