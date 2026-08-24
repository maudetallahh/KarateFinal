using KarateFinal.Data;
using KarateFinal.Models;
using Microsoft.AspNetCore.Mvc;

namespace KarateFinal.Controllers
{
    public class OfficialController : Controller
    {
        private readonly KarateContext _context;
        public OfficialController(KarateContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            var officials = _context.Officials.Where(o => o.ClubId == user.ClubId.Value).ToList();
            ViewBag.Officials = officials;
            ViewBag.ClubId = user.ClubId.Value;
            return View();
        }

        [HttpPost]
        public IActionResult Add(Official official)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            official.ClubId = user.ClubId.Value;
            official.Status = "بانتظار الموافقة";
            official.CreatedAt = DateTime.UtcNow;
            _context.Officials.Add(official);
            _context.SaveChanges();
            TempData["Success"] = "تم إرسال الطلب بنجاح — بانتظار موافقة الإدارة";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var official = _context.Officials.Find(id);
            if (official != null) { _context.Officials.Remove(official); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }

        // للأدمن
        public IActionResult AdminIndex()
        {
            var officials = _context.Officials.ToList()
                .Select(o => new {
                    o.Id,
                    o.Name,
                    o.Role,
                    o.Age,
                    o.Gender,
                    o.Classification,
                    o.Specialty,
                    o.Degree,
                    o.Status,
                    o.AdminNote,
                    o.CreatedAt,
                    ClubName = _context.Clubs.FirstOrDefault(c => c.Id == o.ClubId)?.Name ?? "—"
                }).ToList();
            ViewBag.Officials = officials;
            return View();
        }

        [HttpPost]
        public IActionResult Approve([FromBody] ApproveOfficialRequest request)
        {
            var official = _context.Officials.Find(request.Id);
            if (official == null) return Json(new { success = false });
            official.Status = request.Status;
            official.AdminNote = request.Note;
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }

    public class ApproveOfficialRequest
    {
        public int Id { get; set; }
        public string Status { get; set; } = "";
        public string? Note { get; set; }
    }
}