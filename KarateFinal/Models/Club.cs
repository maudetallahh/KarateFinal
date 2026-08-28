namespace KarateFinal.Models
{
    public class Club
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string City { get; set; } = "";
        public string Category { get; set; } = "";
        public string? Description { get; set; }
        public string ManagerName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? LogoImage { get; set; }
        public string? MaleImage { get; set; }
        public string? FemaleImage { get; set; }
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int FoundedYear { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByAdmin { get; set; }
        public string? ReceiptTemplate { get; set; }
    }
}