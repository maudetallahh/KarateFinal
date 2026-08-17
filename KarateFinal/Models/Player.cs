namespace KarateFinal.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public int Age => DateTime.Now.Year - BirthDate.Year -
            (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
        public string Gender { get; set; } = "";
        public string Belt { get; set; } = "";
        public double Weight { get; set; }
        public string Status { get; set; } = "نشط";
        public string HealthStatus { get; set; } = "سليم"; // سليم | مصاب
        public string PlayerStatus { get; set; } = "ملتزم"; // ملتزم | موقوف
        public int ClubId { get; set; }
        public Club? Club { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Email { get; set; }
    }
}