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
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username
                               && u.Password == password);

            if (user == null)
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

            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                user.Password = newPassword;
                user.MustChangePassword = false;

                if (user.ClubId.HasValue)
                {
                    var club = _context.Clubs.Find(user.ClubId.Value);
                    if (club != null) club.Password = newPassword;
                }
                _context.SaveChanges();
            }

            var role = HttpContext.Session.GetString("Role");
            if (role == "Player")
                return Redirect("/Player/Dashboard");
            return Redirect("/Club/Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}