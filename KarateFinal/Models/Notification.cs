namespace KarateFinal.Models
{
    public class AppNotification
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string TargetRole { get; set; } = ""; // Admin | Club | Player | All
        public int? TargetClubId { get; set; }
        public int? TargetPlayerId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}