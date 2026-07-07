using LearningAIIntegrations.Core.Interfaces;
using LearningAIIntegrations.Core.Models;
using Microsoft.Extensions.Logging;

namespace LearningAIIntegrations.Infrastructure.Services
{
    public class MortgageToolsService : IMortgageToolsService
    {
        private readonly ILogger<MortgageToolsService> _logger;

        public MortgageToolsService(ILogger<MortgageToolsService> logger)
        {
            _logger = logger;
        }

        // ── Tool 1: Calculate Monthly Payment ────────────────────
        // Standard mortgage amortization formula:
        // M = P[r(1+r)^n] / [(1+r)^n - 1]
        public MortgagePaymentResult CalculateMortgagePayment(
            decimal loanAmount,
            double annualRate,
            int termYears)
        {
            _logger.LogInformation(
                "Calculating mortgage payment: Amount={Amount}, Rate={Rate}%, Years={Years}",
                loanAmount, annualRate, termYears);

            // Convert annual rate to monthly decimal
            // e.g. 6.5% annual → 0.065 / 12 = 0.00542 monthly
            var monthlyRate = annualRate / 100 / 12;

            // Total number of monthly payments
            // e.g. 30 years → 360 payments
            var totalPayments = termYears * 12;

            decimal monthlyPayment;

            // Edge case — 0% interest loan (rare but possible)
            if (monthlyRate == 0)
            {
                monthlyPayment = loanAmount / totalPayments;
            }
            else
            {
                // Standard amortization formula
                // Math.Pow = (1 + r)^n
                var factor = Math.Pow(1 + monthlyRate, totalPayments);
                monthlyPayment = loanAmount *
                    (decimal)(monthlyRate * factor / (factor - 1));
            }

            // Round to 2 decimal places — we are dealing with money
            monthlyPayment = Math.Round(monthlyPayment, 2);

            var totalPayment = Math.Round(monthlyPayment * totalPayments, 2);
            var totalInterest = Math.Round(totalPayment - loanAmount, 2);

            _logger.LogInformation(
                "Monthly payment calculated: ${Payment}", monthlyPayment);

            return new MortgagePaymentResult
            {
                MonthlyPayment = monthlyPayment,
                TotalPayment = totalPayment,
                TotalInterest = totalInterest,
                LoanAmount = loanAmount,
                AnnualRate = annualRate,
                TermYears = termYears
            };
        }

        // ── Tool 2: Calculate Affordability ──────────────────────
        // Based on the 28/36 DTI rule
        public AffordabilityResult CalculateAffordability(
            decimal annualIncome,
            decimal monthlyDebts,
            decimal downPayment)
        {
            _logger.LogInformation(
                "Calculating affordability: Income={Income}, Debts={Debts}, Down={Down}",
                annualIncome, monthlyDebts, downPayment);

            var monthlyIncome = annualIncome / 12;

            // 28% rule → max monthly HOUSING cost
            var maxByFrontEnd = Math.Round(monthlyIncome * 0.28m, 2);

            // 36% rule → max total debt including housing
            // So max housing = 36% of income MINUS existing debts
            var maxByBackEnd = Math.Round(
                (monthlyIncome * 0.36m) - monthlyDebts, 2);

            // Use the LOWER of the two — more conservative = safer
            var maxMonthlyPayment = Math.Min(maxByFrontEnd, maxByBackEnd);

            // Guard — if debts are already too high, they can't afford anything
            if (maxMonthlyPayment <= 0)
            {
                return new AffordabilityResult
                {
                    MaxHomePrice = 0,
                    MaxMonthlyPayment = 0,
                    MaxLoanAmount = 0,
                    DebtToIncomeRatio = (double)(monthlyDebts / monthlyIncome * 100),
                    Recommendation = "Current debt levels are too high. " +
                                        "Reduce existing debts before applying for a mortgage."
                };
            }

            // Work backwards from max monthly payment to max loan amount
            // Using standard 30yr at 7% as a baseline for affordability estimate
            // This is what most lenders use for quick estimates
            var assumedRate = 0.07 / 12; // 7% annual → monthly
            var totalPayments = 360;        // 30 years

            var factor = Math.Pow(1 + assumedRate, totalPayments);

            // Reverse of the mortgage formula — solve for P (principal)
            var maxLoanAmount = Math.Round(
                maxMonthlyPayment * (decimal)((factor - 1) / (assumedRate * factor)), 2);

            var maxHomePrice = Math.Round(maxLoanAmount + downPayment, 2);
            var debtToIncomeRatio = Math.Round(
                (double)(monthlyDebts / monthlyIncome * 100), 1);

            string recommendation;
            if (debtToIncomeRatio < 20)
                recommendation = "Excellent DTI ratio. You are in a strong position to qualify.";
            else if (debtToIncomeRatio < 36)
                recommendation = "Good DTI ratio. You should qualify for most loan programs.";
            else
                recommendation = "High DTI ratio. Consider paying down debts to improve qualification chances.";

            _logger.LogInformation(
                "Max home price: ${Price}", maxHomePrice);

            return new AffordabilityResult
            {
                MaxHomePrice = maxHomePrice,
                MaxMonthlyPayment = maxMonthlyPayment,
                MaxLoanAmount = maxLoanAmount,
                DebtToIncomeRatio = debtToIncomeRatio,
                Recommendation = recommendation
            };
        }

        // ── Tool 3: Get Qualified Loan Types ─────────────────────
        // Based on credit score + down payment rules
        public LoanTypesResult GetQualifiedLoanTypes(
            int creditScore,
            double downPaymentPercent)
        {
            _logger.LogInformation(
                "Getting loan types: CreditScore={Score}, Down={Down}%",
                creditScore, downPaymentPercent);

            var qualifiedLoans = new List<LoanOption>();

            // FHA Loan — most accessible, government backed
            // 580+ credit with 3.5% down
            // 500-579 credit with 10% down
            if (creditScore >= 580 && downPaymentPercent >= 3.5 ||
                creditScore >= 500 && downPaymentPercent >= 10)
            {
                qualifiedLoans.Add(new LoanOption
                {
                    LoanType = "FHA Loan",
                    Description = "Government-backed loan. " +
                                    "Great for first-time buyers with lower credit scores. " +
                                    "Requires mortgage insurance premium (MIP).",
                    MinDownPayment = creditScore >= 580 ? 3.5 : 10.0,
                    RequiresPMI = true
                });
            }

            // Conventional Loan — most common
            // 620+ credit, 3% down minimum
            if (creditScore >= 620 && downPaymentPercent >= 3)
            {
                qualifiedLoans.Add(new LoanOption
                {
                    LoanType = "Conventional Loan",
                    Description = "Standard mortgage not backed by government. " +
                                    "Best rates for good credit. " +
                                    "PMI required if down payment < 20%.",
                    MinDownPayment = 3.0,
                    RequiresPMI = downPaymentPercent < 20
                });
            }

            // VA Loan — veterans only, best terms
            // No minimum credit score federally (lenders set own minimums)
            // 0% down payment
            if (creditScore >= 580)
            {
                qualifiedLoans.Add(new LoanOption
                {
                    LoanType = "VA Loan (Veterans Only)",
                    Description = "Exclusively for eligible veterans and service members. " +
                                    "No down payment required. " +
                                    "No PMI. Best rates available.",
                    MinDownPayment = 0,
                    RequiresPMI = false
                });
            }

            // Jumbo Loan — for high value properties
            // 700+ credit, 20% down, loan > $726,200 (2024 conforming limit)
            if (creditScore >= 700 && downPaymentPercent >= 20)
            {
                qualifiedLoans.Add(new LoanOption
                {
                    LoanType = "Jumbo Loan",
                    Description = "For loan amounts above $726,200. " +
                                    "Requires excellent credit and large down payment. " +
                                    "No PMI typically required.",
                    MinDownPayment = 20.0,
                    RequiresPMI = false
                });
            }

            return new LoanTypesResult
            {
                QualifiedLoans = qualifiedLoans,
                CreditScore = creditScore,
                DownPaymentPercent = downPaymentPercent
            };
        }
    }
}