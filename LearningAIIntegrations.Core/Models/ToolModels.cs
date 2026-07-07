namespace LearningAIIntegrations.Core.Models
{
    // ── What frontend sends ───────────────────────────────────────
    // User just asks a natural language question
    // AI decides which tool to call — user doesn't need to know
    // about tools at all
    public class MortgageAIRequest
    {
        public string Question { get; set; } = string.Empty;
    }

    // ── What we send back ─────────────────────────────────────────
    public class MortgageAIResponse
    {
        // The final answer from AI after tool execution
        public string Answer { get; set; } = string.Empty;

        // Which tool was called — great for debugging + transparency
        // e.g. "CalculateMortgagePayment"
        // null if AI answered without needing a tool
        public string? ToolCalled { get; set; }

        // The arguments AI passed to the tool
        // e.g. { "loanAmount": 300000, "annualRate": 6.5, "termYears": 30 }
        public List<Dictionary<string, object>>? ToolArguments { get; set; }

        // The raw result from the tool before AI processed it
        // e.g. "1896.20"
        public string? ToolResult { get; set; }
    }

    // ── Mortgage calculation results ──────────────────────────────
    // These are returned by our tool functions
    // then serialized to string and sent back to AI

    public class MortgagePaymentResult
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalPayment { get; set; }       // over full term
        public decimal TotalInterest { get; set; }      // total interest paid
        public decimal LoanAmount { get; set; }
        public double AnnualRate { get; set; }
        public int TermYears { get; set; }
    }

    public class AffordabilityResult
    {
        public decimal MaxHomePrice { get; set; }
        public decimal MaxMonthlyPayment { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public double DebtToIncomeRatio { get; set; }   // current DTI
        public string Recommendation { get; set; } = string.Empty;
    }

    public class LoanTypesResult
    {
        // List of loan types they qualify for
        public List<LoanOption> QualifiedLoans { get; set; } = new();
        public int CreditScore { get; set; }
        public double DownPaymentPercent { get; set; }
    }

    public class LoanOption
    {
        public string LoanType { get; set; } = string.Empty;    // "FHA", "Conventional"
        public string Description { get; set; } = string.Empty;
        public double MinDownPayment { get; set; }
        public bool RequiresPMI { get; set; }                   // Private Mortgage Insurance
    }
}