using KarateFinal.Data;
using KarateFinal.Models;
using Microsoft.AspNetCore.Mvc;

namespace KarateFinal.Controllers
{
    public class AccountController : Controller
    {
        private readonly KarateContext _context;

        public AccountController(KarateContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            var site = _context.SiteSettings.FirstOrDefault();
            ViewBag.LogoPath = site?.LogoPath ?? "/images/test.jpg";
            ViewBag.SiteName = site?.SiteName ?? "منصة الكاراتيه الفلسطينية";
            ViewBag.TabName = site?.TabName ?? "منصة الكاراتيه";
            ViewBag.Slogan = site?.Slogan ?? "اصنع تاريخك ...وكن بطلاً";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }

            // تسجيل آخر دخول
            user.LastLogin = DateTime.Now;
            _context.SaveChanges();

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            if (user.ClubId.HasValue)
                HttpContext.Session.SetString("ClubId", user.ClubId.Value.ToString());

            if (user.MustChangePassword)
                return Redirect("/Account/ChangePassword");

            if (user.Role == "Admin")
                return Redirect("/Admin/Index");
            else if (user.Role == "Club")
                return Redirect("/Club/Dashboard");
            else if (user.Role == "Player")
                return Redirect("/Player/Dashboard");
            else
                return Redirect("/Account/Login");
        }

        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Redirect("/Account/Login");
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "كلمتا المرور غير متطابقتين";
                return View();
            }

            // سياسة كلمة المرور
            if (newPassword.Length < 8)
            {
                ViewBag.Error = "كلمة المرور يجب أن تكون 8 أحرف على الأقل";
                return View();
            }
            if (!newPassword.Any(char.IsUpper))
            {
                ViewBag.Error = "كلمة المرور يجب أن تحتوي على حرف كبير";
                return View();
            }
            if (!newPassword.Any(char.IsDigit))
            {
                ViewBag.Error = "كلمة المرور يجب أن تحتوي على رقم";
                return View();
            }
            if (!newPassword.Any(c => "!@#$%^&*".Contains(c)))
            {
                ViewBag.Error = "كلمة المرور يجب أن تحتوي على رمز خاص (!@#$%^&*)";
                return View();
            }

            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                // تشفير كلمة المرور
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.MustChangePassword = false;

                if (user.ClubId.HasValue)
                {
                    var club = _context.Clubs.Find(user.ClubId.Value);
                    if (club != null) club.Password = user.Password;
                }

                if (user.PlayerId.HasValue)
                {
                    var player = _context.Players.Find(user.PlayerId.Value);
                    if (player != null) player.Password = user.Password;
                }

                _context.SaveChanges();
            }

            var role = HttpContext.Session.GetString("Role");
            if (role == "Player")
                return Redirect("/Player/Dashboard");
            else if (role == "Admin")
                return Redirect("/Admin/Index");
            return Redirect("/Club/Dashboard");
        }
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string newUsername, string newPassword, string email)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(newUsername))
            {
                user.Username = newUsername;
                HttpContext.Session.SetString("Username", newUsername);
            }
            if (!string.IsNullOrEmpty(newPassword))
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            if (!string.IsNullOrEmpty(email))
                user.Email = email;

            _context.SaveChanges();
            TempData["Success"] = "تم تحديث المعلومات بنجاح ✅";
            return RedirectToAction("Profile");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}