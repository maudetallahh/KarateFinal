using KarateFinal.Data;
using KarateFinal.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KarateFinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly KarateContext _context;

        public HomeController(ILogger<HomeController> logger, KarateContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var site = _context.SiteSettings.FirstOrDefault();
            ViewBag.SiteName = site?.SiteName ?? "منصة الكاراتيه الفلسطينية";
            ViewBag.Slogan = site?.Slogan ?? "اصنع تاريخك ...وكن بطلاً";
            ViewBag.LogoPath = site?.LogoPath ?? "/images/test.jpg";
            ViewBag.TabName = site?.TabName ?? "منصة الكاراتيه";
            ViewBag.ClubsCount = _context.Clubs.Count();
            ViewBag.PlayersCount = _context.Players.Count();
            ViewBag.TournamentsCount = _context.Tournaments.Count();
            ViewBag.UpcomingTournaments = _context.Tournaments
     .Where(t => t.Date >= DateTime.Today)
     .OrderBy(t => t.Date).Take(6).ToList();

            ViewBag.PastTournaments = _context.Tournaments
                .Where(t => t.Date < DateTime.Today)
                .OrderByDescending(t => t.Date).Take(6).ToList();
            ViewBag.TopClubs = _context.Participations
                .GroupBy(p => p.ClubId)
                .Select(g => new { ClubId = g.Key, TotalPoints = g.Sum(p => p.Points) })
                .OrderByDescending(x => x.TotalPoints).Take(5).ToList()
                .Select(x => new {
                    ClubName = _context.Clubs.FirstOrDefault(c => c.Id == x.ClubId)?.Name ?? "—",
                    x.TotalPoints
                }).ToList();
            return View();
        }
        public IActionResult About()
        {
            var site = _context.SiteSettings.FirstOrDefault();
            ViewBag.SiteName = site?.SiteName ?? "منصة الكاراتيه الفلسطينية";
            ViewBag.LogoPath = site?.LogoPath ?? "/images/test.jpg";
            ViewBag.ClubsCount = _context.Clubs.Count();
            ViewBag.PlayersCount = _context.Players.Count();
            ViewBag.TournamentsCount = _context.Tournaments.Count();
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}