namespace KarateFinal.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderRole { get; set; } = ""; // Admin, Club, Player, Official
        public int? SenderClubId { get; set; }
        public int? SenderPlayerId { get; set; }
        public int? SenderOfficialId { get; set; }
        public string ReceiverRole { get; set; } = ""; // Admin, Club, Player, All
        public int? ReceiverClubId { get; set; }
        public int? ReceiverPlayerId { get; set; }
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool IsGroupMessage { get; set; } = false;
    }
}