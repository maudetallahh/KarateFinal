namespace KarateFinal.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string City { get; set; } = "";
        public string Description { get; set; } = "";
        public string Location { get; set; } = "";
        public string Gender { get; set; } = "الكل";
        public decimal RegistrationFee { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public int MaxPlayersPerClub { get; set; }
        public string Categories { get; set; } = "";
        public bool RegistrationClosed { get; set; } = false;
    }
}