namespace KarateFinal.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public Club? Club { get; set; }
        public int Year { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = "غير مدفوع";
        public DateTime? PaidDate { get; set; }
        public string? PaymentMethod { get; set; } // Online, Manual
        public string? TransactionId { get; set; }
    }
}