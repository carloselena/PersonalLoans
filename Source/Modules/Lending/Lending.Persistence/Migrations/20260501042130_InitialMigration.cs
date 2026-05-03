using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lending.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lending");

            migrationBuilder.CreateTable(
                name: "loans",
                schema: "lending",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_rate = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    rate_period = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    penalty_interest_rate = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    penalty_rate_period = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    term = table.Column<int>(type: "integer", nullable: false),
                    payment_frequency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    disbursed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    one_time_interest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    one_time_due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    lender_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loans", x => x.id);
                    table.CheckConstraint("ck_loans_disbursed_after_created", "disbursed_at IS NULL OR disbursed_at >= created_at");
                    table.CheckConstraint("ck_loans_interest_rate_non_negative", "interest_rate >= 0 AND interest_rate < 1");
                    table.CheckConstraint("ck_loans_one_time_due_date_after_created", "one_time_due_date IS NULL OR one_time_due_date >= (created_at AT TIME ZONE 'America/Santo_Domingo')::date");
                    table.CheckConstraint("ck_loans_one_time_interest_non_negative", "one_time_interest IS NULL OR one_time_interest >= 0");
                    table.CheckConstraint("ck_loans_payment_frequency_valid", "payment_frequency IN ('Daily','Weekly','BiWeekly','Monthly','OneTime')");
                    table.CheckConstraint("ck_loans_penalty_interest_rate_positive", "penalty_interest_rate > 0 AND penalty_interest_rate < 1");
                    table.CheckConstraint("ck_loans_penalty_rate_period_valid", "penalty_rate_period IN ('Weekly','Monthly','Annual')");
                    table.CheckConstraint("ck_loans_principal_positive", "principal > 0");
                    table.CheckConstraint("ck_loans_rate_period_valid", "rate_period IN ('Weekly','Monthly','Annual')");
                    table.CheckConstraint("ck_loans_status_valid", "status IN ('Draft','Active','PaidOff')");
                    table.CheckConstraint("ck_loans_term_positive", "term > 0");
                });

            migrationBuilder.CreateTable(
                name: "installments",
                schema: "lending",
                columns: table => new
                {
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_installments", x => new { x.loan_id, x.number });
                    table.CheckConstraint("ck_installments_amount_paid_lte_amount", "amount_paid <= amount");
                    table.CheckConstraint("ck_installments_amount_paid_non_negative", "amount_paid >= 0");
                    table.CheckConstraint("ck_installments_amount_positive", "amount > 0");
                    table.CheckConstraint("ck_installments_interest_non_negative", "interest >= 0");
                    table.CheckConstraint("ck_installments_number_positive", "number > 0");
                    table.CheckConstraint("ck_installments_principal_non_negative", "principal >= 0");
                    table.ForeignKey(
                        name: "fk_installments_loans_loan_id",
                        column: x => x.loan_id,
                        principalSchema: "lending",
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "penalties",
                schema: "lending",
                columns: table => new
                {
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_number = table.Column<int>(type: "integer", nullable: false),
                    applied_at = table.Column<DateOnly>(type: "date", nullable: false),
                    last_applied_at = table.Column<DateOnly>(type: "date", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_penalties", x => x.id);
                    table.CheckConstraint("ck_penalties_amount_paid_non_negative", "amount_paid >= 0");
                    table.CheckConstraint("ck_penalties_amount_positive", "amount > 0");
                    table.CheckConstraint("ck_penalties_installment_number_positive", "installment_number > 0");
                    table.CheckConstraint("ck_penalties_no_overpayment", "amount_paid <= amount");
                    table.ForeignKey(
                        name: "fk_penalties_loans_loan_id",
                        column: x => x.loan_id,
                        principalSchema: "lending",
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_installments_loan_id_due_date",
                schema: "lending",
                table: "installments",
                columns: new[] { "loan_id", "due_date" });

            migrationBuilder.CreateIndex(
                name: "ix_loans_client_id_created_at",
                schema: "lending",
                table: "loans",
                columns: new[] { "client_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_loans_created_at",
                schema: "lending",
                table: "loans",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_loans_disbursed_at",
                schema: "lending",
                table: "loans",
                column: "disbursed_at",
                filter: "disbursed_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_loans_lender_id",
                schema: "lending",
                table: "loans",
                column: "lender_id");

            migrationBuilder.CreateIndex(
                name: "ix_penalties_applied_at",
                schema: "lending",
                table: "penalties",
                column: "applied_at");

            migrationBuilder.CreateIndex(
                name: "ix_penalties_installment_number",
                schema: "lending",
                table: "penalties",
                column: "installment_number");

            migrationBuilder.CreateIndex(
                name: "ix_penalties_loan_id",
                schema: "lending",
                table: "penalties",
                column: "loan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "installments",
                schema: "lending");

            migrationBuilder.DropTable(
                name: "penalties",
                schema: "lending");

            migrationBuilder.DropTable(
                name: "loans",
                schema: "lending");
        }
    }
}
