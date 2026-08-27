namespace KarateFinal.Models
{
    public class PaymentReceipt
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public int ClubId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = ""; // اسم النادي
    }
}