namespace KarateFinal.Models
{
    public class Official
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public Club? Club { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = ""; // حكم، مدرب، إداري، مساعد مدرب
        public int Age { get; set; }
        public string Gender { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Classification { get; set; } // دولي، آسيوي، عربي، وطني
        public string? Specialty { get; set; } // كاتا، كوميتيه (للحكام)
        public string? Degree { get; set; } // الدرجة
        public DateTime? JoinDate { get; set; }
        public string Status { get; set; } = "بانتظار الموافقة"; // بانتظار الموافقة، موافق، مرفوض
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}