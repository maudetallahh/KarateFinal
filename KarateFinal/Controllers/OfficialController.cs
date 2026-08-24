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
            official.CreatedAt = DateTime.UtcNow;

            // مساعد مدرب موافقة تلقائية
            official.Status = official.Role == "مساعد مدرب" ? "موافق" : "بانتظار الموافقة";

            _context.Officials.Add(official);
            _context.SaveChanges();

            if (official.Role == "مساعد مدرب")
                TempData["Success"] = "تم إضافة مساعد المدرب بنجاح";
            else
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

        public IActionResult AdminIndex()
        {
            var officials = _context.Officials.ToList()
                .Select(o => new {
                    o.Id,
                    o.Name,
                    o.Role,
                    o.Age,
                    o.Gender,
                    o.Email,
                    o.Phone,
                    o.Classification,
                    o.Specialty,
                    o.Degree,
                    o.Status,
                    o.AdminNote,
                    o.CreatedAt,
                    o.Username,
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

            if (request.Status == "موافق" && official.Role != "مساعد مدرب" && string.IsNullOrEmpty(official.Username))
            {
                var chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
                var rng = new Random();
                var plainPassword = "Off@" + new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
                var prefix = official.Role switch
                {
                    "حكم" => "referee",
                    "مدرب" => "coach",
                    "إداري" => "admin_club",
                    _ => "official"
                };
                var username = prefix + official.Id;
                official.Username = username;
                official.Password = plainPassword;
                official.MustChangePassword = true;
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(official.Email))
                {
                    var emailTo = official.Email;
                    var emailName = official.Name;
                    var emailSubject = "تم قبول طلبك — منصة الكاراتيه الفلسطينية";
                    var emailBody = "<div dir='rtl' style='font-family:Arial;padding:20px;'><h2>مرحباً " + official.Name + " 🎉</h2><p>تم قبول طلبك كـ <strong>" + official.Role + "</strong> في منصة الكاراتيه الفلسطينية.</p><div style='background:#f8fafc;padding:16px;border-radius:8px;border:1px solid #e2e8f0;margin:16px 0;'><p><strong>اسم المستخدم:</strong> " + username + "</p><p><strong>كلمة المرور:</strong> " + plainPassword + "</p></div><p style='color:#888;font-size:12px;'>يرجى تغيير كلمة المرور عند أول تسجيل دخول.</p></div>";
                    var emailService = HttpContext.RequestServices.GetRequiredService<KarateFinal.Services.EmailService>();
                    _ = Task.Run(async () =>
                    {
                        try { await emailService.SendAsync(emailTo, emailName, emailSubject, emailBody); }
                        catch (Exception ex) { Console.WriteLine("Email error: " + ex.Message); }
                    });
                }
            }
            else
            {
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            if (role != "Official") return RedirectToAction("Login", "Account");
            return View();
        }
    }

    public class ApproveOfficialRequest
    {
        public int Id { get; set; }
        public string Status { get; set; } = "";
        public string? Note { get; set; }
    }
}