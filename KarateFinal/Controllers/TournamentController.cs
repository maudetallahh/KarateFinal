using KarateFinal.Data;
using Microsoft.AspNetCore.Mvc;

namespace KarateFinal.Controllers
{
    public class TournamentController : Controller
    {
        private readonly KarateContext _context;

        public TournamentController(KarateContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Tournaments = _context.Tournaments.ToList();
            return View();
        }
    }
}