namespace KarateFinal.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int? ClubId { get; set; }
        public Club? Club { get; set; }
        public string SenderRole { get; set; } = ""; // Admin, Club
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}