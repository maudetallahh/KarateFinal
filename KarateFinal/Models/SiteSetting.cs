namespace KarateFinal.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }
        public string SiteName { get; set; } = "منصة الكاراتيه الفلسطينية";
        public string TabName { get; set; } = "منصة الكاراتيه";
        public string Slogan { get; set; } = "اصنع تاريخك ...وكن بطلاً";
        public string? LogoPath { get; set; }
        public string? FaviconPath { get; set; }
    }
}