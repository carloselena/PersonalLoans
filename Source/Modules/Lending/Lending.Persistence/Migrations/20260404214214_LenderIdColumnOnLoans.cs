using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lending.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LenderIdColumnOnLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "lender_id",
                schema: "lending",
                table: "loans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lender_id",
                schema: "lending",
                table: "loans");
        }
    }
}
