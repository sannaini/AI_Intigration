using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Core.Interfaces
{
    // ── Contract for mortgage calculation tools ───────────────────
    //
    // These are the actual functions the AI can "call"
    // When AI decides to use a tool it returns the function name
    // and arguments — YOUR code calls the matching method here
    //
    // Think of this as your "tool registry"
    // Every method here = one tool the AI knows about
    public interface IMortgageToolsService
    {
        // ── Tool 1: Calculate Monthly Payment ────────────────────
        // The most common mortgage question
        //
        // Formula: M = P[r(1+r)^n]/[(1+r)^n-1]
        //   M = monthly payment
        //   P = principal loan amount
        //   r = monthly interest rate (annual / 12)
        //   n = total number of payments (years × 12)
        //
        // Example:
        //   loanAmount:  300,000
        //   annualRate:  6.5 (%)
        //   termYears:   30
        //   → monthly:   $1,896.20
        MortgagePaymentResult CalculateMortgagePayment(
            decimal loanAmount,
            double annualRate,
            int termYears);

        // ── Tool 2: Calculate Affordability ──────────────────────
        // "How much house can I afford?"
        // Based on the 28/36 DTI rule used by most lenders:
        //   28% rule → housing costs ≤ 28% of gross monthly income
        //   36% rule → total debts  ≤ 36% of gross monthly income
        //
        // Example:
        //   annualIncome:  80,000
        //   monthlyDebts:  500 (car payment, student loans etc)
        //   downPayment:   40,000
        //   → maxHomePrice: ~$280,000
        AffordabilityResult CalculateAffordability(
            decimal annualIncome,
            decimal monthlyDebts,
            decimal downPayment);

        // ── Tool 3: Get Qualified Loan Types ─────────────────────
        // "What loan types do I qualify for?"
        // Based on credit score + down payment percentage:
        //
        //   FHA loan:          580+ credit, 3.5% down
        //   Conventional:      620+ credit, 3% down
        //   VA loan:           No minimum, 0% down (veterans only)
        //   Jumbo loan:        700+ credit, 20% down
        //
        // Example:
        //   creditScore:         650
        //   downPaymentPercent:  5
        //   → qualifies for: FHA, Conventional
        LoanTypesResult GetQualifiedLoanTypes(
            int creditScore,
            double downPaymentPercent);
    }
}