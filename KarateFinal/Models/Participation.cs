namespace KarateFinal.Models
{
    public class Participation
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public Club? Club { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int Rank { get; set; }
        public int Points { get; set; }
        public string Result { get; set; } = "";
    }
}