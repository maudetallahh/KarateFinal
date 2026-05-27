namespace KarateFinal.Models
{
    public class PlayerMembership
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public int ClubId { get; set; }
        public int Year { get; set; }
        public decimal MonthlyFee { get; set; } = 150;
        public decimal OldDebt { get; set; } = 0;
        public string PaidMonths { get; set; } = "";
    }
}