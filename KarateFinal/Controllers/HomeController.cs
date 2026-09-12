using KarateFinal.Data;
using KarateFinal.Models;
using Microsoft.AspNetCore.Mvc;
namespace KarateFinal.Controllers;
using Microsoft.EntityFrameworkCore;

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
        ViewBag.ClubsCount = _context.Clubs.Count(c => !c.IsDeleted);
        ViewBag.PlayersCount = _context.Players.Count();
        ViewBag.TournamentsCount = _context.Tournaments.Count();
        var today = DateTime.UtcNow.Date;
        ViewBag.UpcomingTournaments = _context.Tournaments
            .Where(t => t.Date >= today)
            .OrderBy(t => t.Date)
            .Take(6)
            .ToList();
        ViewBag.PastTournaments = _context.Tournaments
            .Where(t => t.Date < today)
            .OrderByDescending(t => t.Date)
            .Take(6)
            .ToList();
        ViewBag.TopClubs = _context.Participations
            .GroupBy(p => p.ClubId)
            .Select(g => new { ClubId = g.Key, TotalPoints = g.Sum(p => p.Points) })
            .OrderByDescending(x => x.TotalPoints)
            .Take(5)
            .ToList()
            .Select(x => new {
                ClubName = _context.Clubs.FirstOrDefault(c => c.Id == x.ClubId)?.Name ?? "—",
                x.TotalPoints
            }).ToList();
        return View();
    }
   
  
    public IActionResult PlayerDetails(int id)
    {
        // 1. جلب بيانات اللاعب مع النادي الخاص به
        var player = _context.Players
            .Include(p => p.Club)
            .FirstOrDefault(p => p.Id == id && p.IsNationalTeam);

        if (player == null) return NotFound();

        // 2. حساب الفئة العمرية
        var age = player.Age;
        var category = age <= 10 ? "أشبال" : age <= 11 ? "ناشئ أ" : age <= 13 ? "ناشئ ب" : age <= 15 ? "كاديت" : age <= 17 ? "جونيور" : "سينيور";

        // 3. جلب مشاركات النادي التابع له اللاعب
        var results = _context.Participations
            .Where(r => r.ClubId == player.ClubId)
            .Select(r => new
            {
                Tournament = r.Tournament != null ? r.Tournament.Title : "",
                rank = r.Rank,
                points = r.Points
            })
            .ToList();

        var totalPoints = results.Sum(r => r.points);

        // 4. إرجاع النتيجة كـ JSON
        return Json(new
        {
            name = player.Name,
            club = player.Club != null ? player.Club.Name : "",
            belt = player.Belt,
            category,
            totalPoints,
            results
        });
    }
    public IActionResult About()
    {
        var site = _context.SiteSettings.FirstOrDefault();
        ViewBag.SiteName = site?.SiteName ?? "منصة الكاراتيه الفلسطينية";
        ViewBag.LogoPath = site?.LogoPath ?? "/images/test.jpg";
        ViewBag.ClubsCount = _context.Clubs.Count(c => !c.IsDeleted);
        ViewBag.PlayersCount = _context.Players.Count();
        ViewBag.TournamentsCount = _context.Tournaments.Count();
        return View();
    }
    public IActionResult NationalTeam()
    {
        var players = _context.Players
            .Include(p => p.Club)
            .Where(p => p.IsNationalTeam && p.PlayerStatus != "موقوف")
            .ToList();

        ViewBag.Players = players;
        return View();
    }
    public IActionResult TournamentDetails(int id)
    {
        var tournament = _context.Tournaments.Find(id);
        if (tournament == null) return NotFound();
        var registrations = _context.TournamentPlayerRequests
            .Where(r => r.TournamentId == id && r.Status == "موافق")
            .Include(r => r.Player)
            .ToList();
        var logo = _context.SiteSettings.FirstOrDefault();
        ViewBag.Tournament = tournament;
        ViewBag.Registrations = registrations;
        ViewBag.SiteLogo = logo?.LogoPath;
        return View();
    }
    public IActionResult ClubsRanking()
    {
        var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
        var points = _context.Participations
            .GroupBy(p => p.ClubId)
            .Select(g => new { ClubId = g.Key, TotalPoints = g.Sum(p => p.Points) })
            .ToList();
        var result = clubs.Select(c => new {
            c.Id,
            c.Name,
            c.City,
            c.Category,
            managerName = c.ManagerName,
            totalPoints = points.FirstOrDefault(p => p.ClubId == c.Id)?.TotalPoints ?? 0,
            logo = c.LogoImage
        }).OrderByDescending(c => c.totalPoints).ToList();
        ViewBag.Clubs = result;
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}