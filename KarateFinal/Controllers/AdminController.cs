using KarateFinal.Data;
using KarateFinal.Models;
using KarateFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static KarateFinal.Controllers.UpdateTournamentRequest;

namespace KarateFinal.Controllers
{
    public class AdminController : Controller
    {
        private readonly KarateContext _context;
        private readonly EmailService _emailService;

        public AdminController(KarateContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            ViewBag.ClubsCount = _context.Clubs.Count();
            ViewBag.PlayersCount = _context.Players.Count();
            ViewBag.TournamentsCount = _context.Tournaments.Count();
            ViewBag.UnpaidCount = _context.Memberships.Count(m => m.Status == "غير مدفوع");
            ViewBag.PaidCount = _context.Memberships.Count(m => m.Status == "مدفوع");
            ViewBag.Clubs = _context.Clubs.ToList();
            ViewBag.Players = _context.Players.ToList();
            ViewBag.Tournaments = _context.Tournaments.ToList();
            ViewBag.Memberships = _context.Memberships.ToList();
            ViewBag.Participations = _context.Participations.ToList();
            ViewBag.LastAdminLogin = _context.Users
                .Where(u => u.Role == "Admin" && u.LastLogin != null)
                .OrderByDescending(u => u.LastLogin)
                .Select(u => u.LastLogin)
                .FirstOrDefault();
            return View();
        }

        public IActionResult AddClub() => View();

        [HttpPost]
        public async Task<IActionResult> AddClub(Club club)
        {
            var citySlug = new Dictionary<string, string>{
                {"نابلس","nablus"},{"رام الله","ramallah"},{"القدس","jerusalem"},
                {"الخليل","hebron"},{"جنين","jenin"},{"طولكرم","tulkarm"},
                {"قلقيلية","qalqilya"},{"بيت لحم","bethlehem"},{"أريحا","jericho"},
                {"سلفيت","salfit"},{"طوباس","tubas"},{"غزة","gaza"}
            };
            var slug = citySlug.ContainsKey(club.City) ? citySlug[club.City] : "club";
            var count = _context.Clubs.Count() + 1;
            club.Username = slug + count;
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var rng = new Random();
            var plainPassword = "Kar@" + new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
            club.Password = plainPassword;
            _context.Clubs.Add(club);
            _context.SaveChanges();
            _context.Users.Add(new User { Username = club.Username, Password = BCrypt.Net.BCrypt.HashPassword(plainPassword), Role = "Club", ClubId = club.Id, MustChangePassword = true });
            _context.SaveChanges();
            var feeSetting = _context.Settings.FirstOrDefault(s => s.Key == "MembershipFee");
            decimal fee = feeSetting != null ? decimal.Parse(feeSetting.Value) : 600;
            _context.Memberships.Add(new Membership { ClubId = club.Id, Year = DateTime.Now.Year, Fee = fee, Status = "غير مدفوع" });
            _context.SaveChanges();

            if (!string.IsNullOrEmpty(club.Email))
            {
                try
                {
                    var subject = "بيانات دخولك إلى منصة الكاراتيه الفلسطينية";
                    var body = "<div dir='rtl' style='font-family:Arial;padding:20px;'>" +
                        "<h2 style='color:#1e2a38;'>مرحباً بنادي " + club.Name + "</h2>" +
                        "<p>تم تسجيل ناديك في منصة الكاراتيه الفلسطينية.</p>" +
                        "<div style='background:#f8fafc;padding:16px;border-radius:8px;border:1px solid #e2e8f0;margin:16px 0;'>" +
                        "<p><strong>اسم المستخدم:</strong> " + club.Username + "</p>" +
                        "<p><strong>كلمة المرور:</strong> " + plainPassword + "</p>" +
                        "</div><p style='color:#888;font-size:12px;'>يرجى تغيير كلمة المرور عند أول تسجيل دخول.</p></div>";
                    await _emailService.SendAsync(club.Email, club.Name, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email error: " + ex.Message);
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult DeleteClub(int id)
        {
            var club = _context.Clubs.Find(id);
            if (club != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.ClubId == id);
                if (user != null) _context.Users.Remove(user);
                var memberships = _context.Memberships.Where(m => m.ClubId == id).ToList();
                _context.Memberships.RemoveRange(memberships);
                _context.Clubs.Remove(club);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetClubPassword([FromBody] ResetPasswordRequest request)
        {
            var club = _context.Clubs.Find(request.ClubId);
            var user = _context.Users.FirstOrDefault(u => u.ClubId == request.ClubId);
            if (club != null) club.Password = request.NewPassword;
            if (user != null) { user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword); user.MustChangePassword = true; }
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult AddPlayer() { ViewBag.Clubs = _context.Clubs.ToList(); return View(); }

        [HttpPost]
        public IActionResult AddPlayer(Player player) { _context.Players.Add(player); _context.SaveChanges(); return RedirectToAction("Index"); }

        public IActionResult DeletePlayer(int id)
        {
            var player = _context.Players.Find(id);
            if (player != null) { _context.Players.Remove(player); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }

        public IActionResult AddTournament() => View();

        [HttpPost]
        public IActionResult AddTournament(Tournament tournament)
        {
            if (tournament.Date < DateTime.Today) { TempData["Error"] = "لا يمكن إضافة بطولة بتاريخ مضى!"; return RedirectToAction("Index"); }
            if (tournament.RegistrationDeadline >= tournament.Date) { TempData["Error"] = "آخر موعد للتسجيل يجب أن يكون قبل تاريخ البطولة!"; return RedirectToAction("Index"); }
            _context.Tournaments.Add(tournament);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteTournament(int id)
        {
            var tournament = _context.Tournaments.Find(id);
            if (tournament != null) { _context.Tournaments.Remove(tournament); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }

        public IActionResult AddParticipation(int? tournamentId)
        {
            ViewBag.Tournaments = _context.Tournaments.ToList();
            ViewBag.SelectedTournament = tournamentId;
            if (tournamentId.HasValue)
            {
                var registeredClubIds = _context.TournamentRegistrations
                   .Where(r => r.TournamentId == tournamentId.Value)
                    .Select(r => r.ClubId).ToList();
                ViewBag.Clubs = _context.Clubs.Where(c => registeredClubIds.Contains(c.Id)).ToList();
            }
            else { ViewBag.Clubs = new List<Club>(); }
            return View();
        }

        [HttpPost]
        public IActionResult AddParticipation(Participation participation) { _context.Participations.Add(participation); _context.SaveChanges(); return RedirectToAction("Index"); }

        public IActionResult DeleteParticipation(int id)
        {
            var p = _context.Participations.Find(id);
            if (p != null) { _context.Participations.Remove(p); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleMembership([FromBody] ToggleMembershipRequest request)
        {
            var membership = _context.Memberships.Find(request.MembershipId);
            if (membership == null) return Json(new { success = false });
            membership.Status = membership.Status == "مدفوع" ? "غير مدفوع" : "مدفوع";
            if (membership.Status == "مدفوع") membership.PaidDate = DateTime.Now;
            _context.SaveChanges();
            return Json(new { success = true, status = membership.Status });
        }

        public IActionResult TournamentDetails(int id)
        {
            var registrations = _context.TournamentRegistrations.Where(r => r.TournamentId == id).Include(r => r.Club).ToList();
            var result = registrations.Select(r => new {
                id = r.Id,
                clubName = r.Club?.Name ?? "—",
                playersCount = r.PlayersCount,
                registeredAt = r.RegisteredAt.ToString("yyyy/MM/dd"),
                status = r.Status,
                adminNote = r.AdminNote ?? "",
                approvedPlayers = _context.TournamentPlayerRequests
                    .Where(p => p.TournamentId == id && p.ClubId == r.ClubId && p.Status == "موافق")
                    .Include(p => p.Player)
                    .Select(p => p.Player.Name)
                    .ToList()
            });
            return Json(result);
        }

        public IActionResult GetApprovedClubs(int tournamentId)
        {
            var clubIds = _context.TournamentRegistrations
.Where(r => r.TournamentId == tournamentId)
.Select(r => r.ClubId).ToList();
            var clubs = _context.Clubs.Where(c => clubIds.Contains(c.Id)).Select(c => new { id = c.Id, name = c.Name }).ToList();
            return Json(clubs);
        }

        [HttpPost]
        public IActionResult ApproveTournamentRegistration([FromBody] TournamentRegistrationActionRequest request)
        {
            var reg = _context.TournamentRegistrations.Find(request.RegistrationId);
            if (reg == null) return Json(new { success = false });
            reg.Status = "موافق";
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RejectTournamentRegistration([FromBody] TournamentRegistrationActionRequest request)
        {
            var reg = _context.TournamentRegistrations.Find(request.RegistrationId);
            if (reg == null) return Json(new { success = false });
            reg.Status = "مرفوض";
            reg.AdminNote = request.Note;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult ToggleTournamentRegistration([FromBody] ToggleTournamentRequest request)
        {
            var tournament = _context.Tournaments.Find(request.TournamentId);
            if (tournament == null) return Json(new { success = false });
            tournament.RegistrationClosed = !tournament.RegistrationClosed;
            _context.SaveChanges();
            return Json(new { success = true, closed = tournament.RegistrationClosed });
        }

        [HttpPost]
        public IActionResult UpdateTournament([FromBody] UpdateTournamentRequest request)
        {
            var tournament = _context.Tournaments.Find(request.Id);
            if (tournament == null) return Json(new { success = false });
            tournament.Title = request.Title;
            tournament.Date = request.Date;
            tournament.City = request.City;
            tournament.Location = request.Location;
            tournament.Description = request.Description;
            tournament.RegistrationFee = request.RegistrationFee;
            tournament.MaxPlayersPerClub = request.MaxPlayersPerClub;
            tournament.Categories = request.Categories;
            _context.SaveChanges();
            return Json(new { success = true });
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
            if (!string.IsNullOrEmpty(newUsername)) { user.Username = newUsername; HttpContext.Session.SetString("Username", newUsername); }
            if (!string.IsNullOrEmpty(newPassword)) user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            if (!string.IsNullOrEmpty(email)) user.Email = email;
            _context.SaveChanges();
            TempData["Success"] = "تم تحديث المعلومات بنجاح";
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public IActionResult UpdateMembershipFee([FromBody] UpdateFeeSettingRequest request)
        {
            var setting = _context.Settings.FirstOrDefault(s => s.Key == "MembershipFee");
            if (setting == null)
                _context.Settings.Add(new Setting { Key = "MembershipFee", Value = request.Fee.ToString() });
            else
                setting.Value = request.Fee.ToString();
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult GetMembershipFee()
        {
            var setting = _context.Settings.FirstOrDefault(s => s.Key == "MembershipFee");
            decimal fee = setting != null ? decimal.Parse(setting.Value) : 600;
            return Json(new { fee });
        }
    }

    public class ResetPasswordRequest { public int ClubId { get; set; } public string NewPassword { get; set; } = ""; }
    public class ToggleMembershipRequest { public int MembershipId { get; set; } }
    public class TournamentRegistrationActionRequest { public int RegistrationId { get; set; } public string? Note { get; set; } }
    public class ToggleTournamentRequest { public int TournamentId { get; set; } }
    public class UpdateTournamentRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string City { get; set; } = "";
        public string Location { get; set; } = "";
        public string? Description { get; set; }
        public decimal RegistrationFee { get; set; }
        public int MaxPlayersPerClub { get; set; }
        public string? Categories { get; set; }
        public class UpdateFeeSettingRequest { public decimal Fee { get; set; } }
    }
}
