using KarateFinal.Data;
using Microsoft.AspNetCore.Mvc;

namespace KarateFinal.Controllers
{
    public class TourController : Controller
    {
        private readonly KarateContext _context;

        public TourController(KarateContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Clubs = _context.Clubs.ToList();
            return View();
        }
    }
}