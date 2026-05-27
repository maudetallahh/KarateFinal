namespace KarateFinal.Models
{
    public class TournamentRegistration
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int ClubId { get; set; }
        public Club? Club { get; set; }
        public int PlayersCount { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "بانتظار الموافقة";
        public string? AdminNote { get; set; }
        public string? AttachedFileName { get; set; }
        public byte[]? AttachedFile { get; set; }
    }
}