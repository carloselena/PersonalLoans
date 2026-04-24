using Lending.Domain.Loans;
using Lending.Domain.Loans.Installments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lending.Persistence.EntityConfigurations;

public class LoanEntityConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans", "lending", t =>
        {
            t.HasCheckConstraint("ck_loans_term_positive", "term > 0");
            
            t.HasCheckConstraint("ck_loans_principal_positive", "principal > 0");
            
            t.HasCheckConstraint("ck_loans_interest_rate_non_negative", "interest_rate >= 0 AND interest_rate < 1");

            t.HasCheckConstraint("ck_loans_penalty_interest_rate_positive", "penalty_interest_rate > 0 AND penalty_interest_rate < 1");
            
            t.HasCheckConstraint("ck_loans_one_time_interest_non_negative",
                "one_time_interest IS NULL OR one_time_interest >= 0");
            
            t.HasCheckConstraint("ck_loans_disbursed_after_created",
                "disbursed_at IS NULL OR disbursed_at >= created_at");
            
            t.HasCheckConstraint("ck_loans_one_time_due_date_after_created",
                "one_time_due_date IS NULL OR one_time_due_date >= created_at::date");
            
            t.HasCheckConstraint("ck_loans_payment_frequency_valid",
                "payment_frequency IN ('Daily','Weekly','BiWeekly','Monthly','OneTime')");
            
            t.HasCheckConstraint("ck_loans_rate_period_valid",
                "rate_period IN ('Weekly','Monthly','Annual')");
            
            t.HasCheckConstraint("ck_loans_penalty_rate_period_valid",
                "penalty_rate_period IN ('Weekly','Monthly','Annual')");
            
            t.HasCheckConstraint("ck_loans_status_valid",
                "status IN ('Draft','Active','PaidOff')");
        });
        
        builder.HasIndex(l => new {l.ClientId, l.CreatedAt});
        builder.HasIndex(l => l.CreatedAt);
        builder.HasIndex(l => l.DisbursedAt)
            .HasFilter("disbursed_at IS NOT NULL");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .HasColumnOrder(0);
        
        builder.Property(l => l.ClientId)
            .HasColumnName("client_id")
            .HasColumnOrder(1)
            .IsRequired();

        builder.ComplexProperty(l => l.Principal, principal =>
        {
            principal.Property(p => p.Amount)
                .HasColumnName("principal")
                .HasPrecision(18, 2)
                .HasColumnOrder(3)
                .IsRequired();
            
            principal.Property(p => p.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3)
                .HasColumnOrder(2)
                .IsRequired();
        });

        builder.ComplexProperty(l => l.InterestRate, interestRate =>
        {
            interestRate.Property(ir => ir.Rate)
                .HasColumnName("interest_rate")
                .HasPrecision(9, 6)
                .HasColumnOrder(4)
                .IsRequired();
            
            interestRate.Property(ir => ir.Period)
                .HasColumnName("rate_period")
                .HasConversion<string>()
                .HasMaxLength(16)
                .HasColumnOrder(5)
                .IsRequired();
        });

        builder.ComplexProperty(l => l.PenaltyInterestRate, penaltyInterestRate =>
        {
            penaltyInterestRate.Property(pir => pir.Rate)
                .HasColumnName("penalty_interest_rate")
                .HasPrecision(9, 6)
                .HasColumnOrder(6)
                .IsRequired();

            penaltyInterestRate.Property(pir => pir.Period)
                .HasColumnName("penalty_rate_period")
                .HasConversion<string>()
                .HasMaxLength(16)
                .HasColumnOrder(7)
                .IsRequired();
        });

        builder.ComplexProperty(l => l.Term, term =>
        {
            term.Property(t => t.Value)
                .HasColumnName("term")
                .HasColumnOrder(8)
                .IsRequired();
        });

        builder.Property(l => l.PaymentFrequency)
            .HasColumnName("payment_frequency")
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasColumnOrder(9)
            .IsRequired();
        
        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnOrder(10)
            .IsRequired();
        
        builder.Property(l => l.DisbursedAt)
            .HasColumnName("disbursed_at")
            .HasColumnOrder(11)
            .IsRequired(false);

        builder.ComplexProperty(l => l.OneTimeInterest, interest =>
        {
            interest.IsRequired(false);
            
            interest.Property(i => i.Amount)
                .HasColumnName("one_time_interest")
                .HasPrecision(18, 2)
                .HasColumnOrder(12);

            interest.Ignore(i => i.Currency);
        });
        
        builder.Property(l => l.OneTimeDueDate)
            .HasColumnName("one_time_due_date")
            .HasColumnOrder(13)
            .IsRequired(false);

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasColumnOrder(14)
            .IsRequired();
        
        builder.Property(l => l.LenderId)
            .HasColumnName("lender_id")
            .HasColumnOrder(15)
            .IsRequired();

        builder.OwnsMany(l => l.Installments, installment =>
        {
            installment.ToTable("installments", "lending", t =>
            {
                t.HasCheckConstraint("ck_installments_number_positive", "number > 0");
                
                t.HasCheckConstraint("ck_installments_amount_positive", "amount > 0");
                
                t.HasCheckConstraint("ck_installments_principal_non_negative", "principal >= 0");
                
                t.HasCheckConstraint("ck_installments_interest_non_negative", "interest >= 0");
                
                t.HasCheckConstraint("ck_installments_amount_paid_non_negative", "amount_paid >= 0");
                
                t.HasCheckConstraint("ck_installments_amount_paid_lte_amount", "amount_paid <= amount");
            });
            
            installment.WithOwner().HasForeignKey("LoanId");
            
            installment.HasKey("LoanId", nameof(Installment.Number));
            installment.HasIndex("LoanId", nameof(Installment.DueDate));

            installment.Property<Guid>("LoanId")
                .HasColumnName("loan_id")
                .HasColumnOrder(0);
            
            installment.Property(i => i.Number)
                .HasColumnName("number")
                .HasColumnOrder(1)
                .IsRequired();


            installment.Property(i => i.DueDate)
                .HasColumnName("due_date")
                .HasColumnOrder(2)
                .IsRequired();
            

            installment.OwnsOne(i => i.Amount, amount =>
            {
                amount.Property(a => a.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2)
                    .HasColumnOrder(4)
                    .IsRequired();
                
                amount.Property(a => a.Currency)
                    .HasColumnName("currency")
                    .HasConversion<string>()
                    .HasMaxLength(3)
                    .HasColumnOrder(3)
                    .IsRequired();
            });

            installment.OwnsOne(i => i.Principal, principal =>
            {
                principal.Property(p => p.Amount)
                    .HasColumnName("principal")
                    .HasPrecision(18, 2)
                    .HasColumnOrder(5)
                    .IsRequired();

                principal.Ignore(p => p.Currency);
            });
            
            installment.OwnsOne(i => i.Interest, interest =>
            {
                interest.Property(i => i.Amount)
                    .HasColumnName("interest")
                    .HasPrecision(18, 2)
                    .HasColumnOrder(6)
                    .IsRequired();

                interest.Ignore(i => i.Currency);
            });
            
            installment.OwnsOne(i => i.AmountPaid, amountPaid =>
            {
                amountPaid.Property(ap => ap.Amount)
                    .HasColumnName("amount_paid")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0)
                    .HasColumnOrder(7)
                    .IsRequired();

                amountPaid.Ignore(ap => ap.Currency);
            });

            builder.Metadata
                .FindNavigation(nameof(Loan.Installments))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

        });

        builder.OwnsMany(l => l.Penalties, penalty =>
        {
            penalty.ToTable("penalties", "lending", t =>
            {
                t.HasCheckConstraint("ck_penalties_amount_positive", "amount > 0");

                t.HasCheckConstraint("ck_penalties_amount_paid_non_negative", "amount_paid >= 0");

                t.HasCheckConstraint("ck_penalties_no_overpayment", "amount_paid <= amount");

                t.HasCheckConstraint("ck_penalties_installment_number_positive", "installment_number > 0");
            });

            penalty.HasIndex("LoanId");
            penalty.HasIndex(p => p.InstallmentNumber);
            penalty.HasIndex(p => p.AppliedAt);
            
            penalty.WithOwner().HasForeignKey("LoanId");
            
            penalty.Property<Guid>("LoanId")
                .HasColumnName("loan_id")
                .HasColumnOrder(0);

            penalty.HasKey("Id");
            penalty.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnName("id")
                .HasColumnOrder(1);
            
            penalty.Property(p => p.InstallmentNumber)
                .HasColumnName("installment_number")
                .HasColumnOrder(2)
                .IsRequired();
            
            penalty.Property(p => p.AppliedAt)
                .HasColumnName("applied_at")
                .HasColumnOrder(3)
                .IsRequired();
            
            penalty.Property(p => p.LastAppliedAt)
                .HasColumnName("last_applied_at")
                .HasColumnOrder(4)
                .IsRequired();

            penalty.OwnsOne(p => p.Amount, amount =>
            {
                amount.Property(a => a.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2)
                    .HasColumnOrder(6)
                    .IsRequired();

                amount.Property(a => a.Currency)
                    .HasColumnName("currency")
                    .HasConversion<string>()
                    .HasMaxLength(3)
                    .HasColumnOrder(5)
                    .IsRequired();
            });

            penalty.OwnsOne(p => p.AmountPaid, amountPaid =>
            {
                amountPaid.Property(ap => ap.Amount)
                    .HasColumnName("amount_paid")
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0)
                    .HasColumnOrder(7)
                    .IsRequired();

                amountPaid.Ignore(ap => ap.Currency);
            });
            
            builder.Metadata
                .FindNavigation(nameof(Loan.Penalties))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}