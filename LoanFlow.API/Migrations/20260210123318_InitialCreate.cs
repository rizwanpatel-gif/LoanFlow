using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "borrowers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ssn = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    annual_income = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    employment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_borrowers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "credit_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    borrower_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    score_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rating = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    debt_to_income_ratio = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    open_accounts = table.Column<int>(type: "integer", nullable: false),
                    delinquencies = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_credit_scores_borrowers_borrower_id",
                        column: x => x.borrower_id,
                        principalTable: "borrowers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loan_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    borrower_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_type = table.Column<string>(type: "text", nullable: false),
                    requested_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    approved_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    interest_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    term_months = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    application_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    decision_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_loan_applications_borrowers_borrower_id",
                        column: x => x.borrower_id,
                        principalTable: "borrowers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    principal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    payment_number = table.Column<int>(type: "integer", nullable: false),
                    remaining_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_loan_applications_loan_application_id",
                        column: x => x.loan_application_id,
                        principalTable: "loan_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_borrowers_email",
                table: "borrowers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_borrowers_ssn",
                table: "borrowers",
                column: "ssn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credit_scores_borrower_id",
                table: "credit_scores",
                column: "borrower_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_application_date",
                table: "loan_applications",
                column: "application_date");

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_borrower_id",
                table: "loan_applications",
                column: "borrower_id");

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_status",
                table: "loan_applications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_payments_due_date",
                table: "payments",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_payments_loan_application_id",
                table: "payments",
                column: "loan_application_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_status",
                table: "payments",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_scores");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "loan_applications");

            migrationBuilder.DropTable(
                name: "borrowers");
        }
    }
}
