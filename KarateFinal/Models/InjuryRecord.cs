namespace KarateFinal.Models
{
    public class InjuryRecord
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public string InjuryNote { get; set; } = "";
        public DateTime InjuryStart { get; set; } = DateTime.Now;
        public DateTime? InjuryEnd { get; set; }
        public string? AttachmentPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}