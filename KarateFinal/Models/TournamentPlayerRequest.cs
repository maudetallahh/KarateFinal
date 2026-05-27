namespace KarateFinal.Models
{
    public class TournamentPlayerRequest
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public int ClubId { get; set; }
        public string Status { get; set; } = "بانتظار الموافقة";
        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }
}