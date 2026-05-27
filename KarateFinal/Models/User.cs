namespace KarateFinal.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public int? ClubId { get; set; }
        public int? PlayerId { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public DateTime? LastLogin { get; set; }
        public string? Email { get; set; }
    }
}