namespace KarateFinal.Models
{
    public class PlayerResult
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public int ClubId { get; set; }
        public string TournamentName { get; set; } = "";
        public int Rank { get; set; }
        public int Points { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}