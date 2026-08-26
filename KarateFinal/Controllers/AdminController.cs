using KarateFinal.Data;
using KarateFinal.Models;
using KarateFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult Clubs()
        {
            var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            ViewBag.Clubs = clubs;
            return View();
        }
        public IActionResult Players()
        {
            var players = _context.Players.ToList();
            var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            var results = _context.PlayerResults.ToList();
            ViewBag.Players = players;
            ViewBag.Clubs = clubs;
            ViewBag.Results = results;
            return View();
        }
        public IActionResult Tournaments()
        {
            var tournaments = _context.Tournaments.ToList();
            ViewBag.Tournaments = tournaments;
            return View();
        }
        public IActionResult Participations()
        {
            var participations = _context.Participations.ToList();
            var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            var tournaments = _context.Tournaments.ToList();
            ViewBag.Participations = participations;
            ViewBag.Clubs = clubs;
            ViewBag.Tournaments = tournaments;
            return View();
        }
        public IActionResult Memberships()
        {
            var memberships = _context.Memberships.ToList();
            var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            ViewBag.Memberships = memberships;
            ViewBag.Clubs = clubs;
            ViewBag.PaidCount = memberships.Count(m => m.Status == "مدفوع");
            ViewBag.UnpaidCount = memberships.Count(m => m.Status == "غير مدفوع");
            return View();
        }
        public IActionResult Archive()
        {
            var archivedClubs = _context.Clubs.Where(c => c.IsDeleted).ToList();
            ViewBag.ArchivedClubs = archivedClubs;
            return View();
        }
        public IActionResult Index()
        {
            ViewBag.ClubsCount = _context.Clubs.Count(c => !c.IsDeleted);
            ViewBag.PlayersCount = _context.Players.Count();
            ViewBag.TournamentsCount = _context.Tournaments.Count();
            ViewBag.UnpaidCount = _context.Memberships.Count(m => m.Status == "غير مدفوع");
            ViewBag.PaidCount = _context.Memberships.Count(m => m.Status == "مدفوع");
            ViewBag.Clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            ViewBag.ArchivedClubs = _context.Clubs.Where(c => c.IsDeleted).ToList();
            ViewBag.Players = _context.Players.ToList();
            ViewBag.Tournaments = _context.Tournaments.ToList();
            ViewBag.Memberships = _context.Memberships.ToList();
            ViewBag.Participations = _context.Participations.ToList();
            ViewBag.LastAdminLogin = _context.Users
                .Where(u => u.Role == "Admin" && u.LastLogin != null)
                .OrderByDescending(u => u.LastLogin)
                .Select(u => u.LastLogin)
                .FirstOrDefault();
            ViewBag.TopClubs = _context.Participations
                .GroupBy(p => p.ClubId)
                .Select(g => new { ClubId = g.Key, TotalPoints = g.Sum(p => p.Points), GoldCount = g.Count(p => p.Rank == 1) })
                .OrderByDescending(x => x.TotalPoints).Take(5).ToList()
                .Select(x => new { ClubName = _context.Clubs.FirstOrDefault(c => c.Id == x.ClubId)?.Name ?? "—", x.TotalPoints, x.GoldCount }).ToList();
            ViewBag.TopPlayers = _context.Players
                .Where(p => _context.Participations.Any(par => par.ClubId == p.ClubId)).ToList()
                .Select(p => new {
                    PlayerName = p.Name,
                    Belt = p.Belt,
                    ClubName = _context.Clubs.FirstOrDefault(c => c.Id == p.ClubId)?.Name ?? "—",
                    TotalPoints = _context.Participations.Where(par => par.ClubId == p.ClubId).Sum(par => par.Points)
                }).OrderByDescending(x => x.TotalPoints).Take(5).ToList();
            return View();
        }
        public IActionResult AddClub() => View();
        [HttpPost]
        public IActionResult AddClub(Club club)
        {
            var citySlug = new Dictionary<string, string>{
                {"نابلس","nablus"},{"رام الله","ramallah"},{"القدس","jerusalem"},
                {"الخليل","hebron"},{"جنين","jenin"},{"طولكرم","tulkarm"},
                {"قلقيلية","qalqilya"},{"بيت لحم","bethlehem"},{"أريحا","jericho"},
                {"سلفيت","salfit"},{"طوباس","tubas"},{"غزة","gaza"}
            };
            if (!string.IsNullOrEmpty(club.Email))
            {
                var emailExists = _context.Clubs.Any(c => c.Email == club.Email && !c.IsDeleted);
                if (emailExists) { TempData["Error"] = "البريد الإلكتروني مسجّل مسبقاً لنادٍ آخر!"; return RedirectToAction("Index"); }
            }
            if (!string.IsNullOrEmpty(club.Phone))
            {
                var phoneExists = _context.Clubs.Any(c => c.Phone == club.Phone && !c.IsDeleted);
                if (phoneExists) { TempData["Error"] = "رقم الهاتف مسجّل مسبقاً لنادٍ آخر!"; return RedirectToAction("Index"); }
            }
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
                var emailTo = club.Email;
                var emailName = club.Name;
                var emailSubject = "بيانات دخولك إلى منصة الكاراتيه الفلسطينية";
                var emailBody = "<div dir='rtl' style='font-family:Arial;padding:20px;'><h2 style='color:#1e2a38;'>مرحباً بنادي " + club.Name + "</h2><p>تم تسجيل ناديك في منصة الكاراتيه الفلسطينية.</p><div style='background:#f8fafc;padding:16px;border-radius:8px;border:1px solid #e2e8f0;margin:16px 0;'><p><strong>اسم المستخدم:</strong> " + club.Username + "</p><p><strong>كلمة المرور:</strong> " + plainPassword + "</p></div><p style='color:#888;font-size:12px;'>يرجى تغيير كلمة المرور عند أول تسجيل دخول.</p></div>";
                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendAsync(emailTo, emailName, emailSubject, emailBody); }
                    catch (Exception ex) { Console.WriteLine("Email error: " + ex.Message); }
                });
            }
            return RedirectToAction("Index");
        }
        public IActionResult DeleteClub(int id)
        {
            var club = _context.Clubs.Find(id);
            if (club != null)
            {
                club.IsDeleted = true;
                club.DeletedAt = DateTime.UtcNow;
                club.DeletedByAdmin = HttpContext.Session.GetString("Username");
                var user = _context.Users.FirstOrDefault(u => u.ClubId == id);
                if (user != null) _context.Users.Remove(user);
                var players = _context.Players.Where(p => p.ClubId == id).ToList();
                foreach (var p in players)
                {
                    var playerUser = _context.Users.FirstOrDefault(u => u.PlayerId == p.Id);
                    if (playerUser != null) _context.Users.Remove(playerUser);
                }
                _context.Players.RemoveRange(players);
                _context.Memberships.RemoveRange(_context.Memberships.Where(m => m.ClubId == id));
                _context.TournamentRegistrations.RemoveRange(_context.TournamentRegistrations.Where(r => r.ClubId == id));
                _context.TournamentPlayerRequests.RemoveRange(_context.TournamentPlayerRequests.Where(r => r.ClubId == id));
                _context.PlayerMemberships.RemoveRange(_context.PlayerMemberships.Where(m => m.ClubId == id));
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
        public IActionResult AddPlayer()
        {
            ViewBag.Clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            return View();
        }
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
                var registeredClubIds = _context.TournamentRegistrations.Where(r => r.TournamentId == tournamentId.Value).Select(r => r.ClubId).ToList();
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
            if (membership.Status == "مدفوع") return Json(new { success = false, message = "لا يمكن إلغاء العضوية بعد الدفع" });
            membership.Status = "مدفوع";
            membership.PaidDate = DateTime.Now;
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
                    .Include(p => p.Player).Select(p => p.Player.Name).ToList()
            });
            return Json(result);
        }
        public IActionResult GetApprovedClubs(int tournamentId)
        {
            var clubIds = _context.TournamentRegistrations.Where(r => r.TournamentId == tournamentId).Select(r => r.ClubId).ToList();
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
        public IActionResult UpdateProfile(string newUsername, string newPassword, string oldPassword, string email)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (string.IsNullOrEmpty(oldPassword) || !BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                { TempData["Error"] = "كلمة المرور القديمة غير صحيحة!"; return RedirectToAction("Profile"); }
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
            if (!string.IsNullOrEmpty(newUsername)) { user.Username = newUsername; HttpContext.Session.SetString("Username", newUsername); }
            if (!string.IsNullOrEmpty(email)) user.Email = email;
            _context.SaveChanges();
            TempData["Success"] = "تم تحديث المعلومات بنجاح";
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public IActionResult UpdateMembershipFee([FromBody] UpdateFeeSettingRequest request)
        {
            var setting = _context.Settings.FirstOrDefault(s => s.Key == "MembershipFee");
            if (setting == null) _context.Settings.Add(new Setting { Key = "MembershipFee", Value = request.Fee.ToString() });
            else setting.Value = request.Fee.ToString();
            _context.SaveChanges();
            return Json(new { success = true });
        }
        public IActionResult GetMembershipFee()
        {
            var setting = _context.Settings.FirstOrDefault(s => s.Key == "MembershipFee");
            decimal fee = setting != null ? decimal.Parse(setting.Value) : 600;
            return Json(new { fee });
        }
        [HttpPost]
        public IActionResult UpdateClubMembershipFee([FromBody] UpdateClubFeeRequest request)
        {
            var membership = _context.Memberships.Find(request.MembershipId);
            if (membership == null) return Json(new { success = false });
            membership.Fee = request.Fee;
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult ResetAllMemberships()
        {
            var currentYear = DateTime.Now.Year;
            var memberships = _context.Memberships.Where(m => m.Year == currentYear).ToList();
            foreach (var m in memberships) { m.Status = "غير مدفوع"; m.PaidDate = null; }
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult CheckClubEmail([FromBody] CheckEmailRequest request)
        {
            var exists = _context.Clubs.Any(c => c.Email == request.Email && !c.IsDeleted);
            return Json(new { exists });
        }
        [HttpPost]
        public IActionResult CheckClubPhone([FromBody] CheckPhoneRequest request)
        {
            var exists = _context.Clubs.Any(c => c.Phone == request.Phone && !c.IsDeleted);
            return Json(new { exists });
        }
        public IActionResult SiteSettings()
        {
            var settings = _context.SiteSettings.FirstOrDefault() ?? new SiteSetting();
            return View(settings);
        }
        [HttpPost]
        public async Task<IActionResult> SiteSettings(SiteSetting model, IFormFile? logoFile, IFormFile? faviconFile)
        {
            var settings = _context.SiteSettings.FirstOrDefault();
            if (settings == null) { settings = new SiteSetting(); _context.SiteSettings.Add(settings); }
            settings.SiteName = model.SiteName;
            settings.TabName = model.TabName;
            settings.Slogan = model.Slogan;
            if (logoFile != null && logoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await logoFile.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                settings.LogoPath = $"data:{logoFile.ContentType};base64,{base64}";
            }
            if (faviconFile != null && faviconFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await faviconFile.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                settings.FaviconPath = $"data:{faviconFile.ContentType};base64,{base64}";
            }
            _context.SaveChanges();
            TempData["Success"] = "تم حفظ معلومات الموقع بنجاح ✅";
            return RedirectToAction("SiteSettings");
        }
        public IActionResult GetSiteSettings()
        {
            var settings = _context.SiteSettings.FirstOrDefault() ?? new SiteSetting();
            return Json(new
            {
                siteName = settings.SiteName,
                tabName = settings.TabName,
                slogan = settings.Slogan,
                logoPath = settings.LogoPath ?? "/images/test.jpg",
                faviconPath = settings.FaviconPath ?? "/images/test.jpg"
            });
        }
        public IActionResult GetNotifications()
        {
            var notifications = _context.Notifications
                .Where(n => n.TargetRole == "Admin" || n.TargetRole == "All")
                .OrderByDescending(n => n.CreatedAt).Take(10)
                .Select(n => new { n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt }).ToList();
            var unread = notifications.Count(n => !n.IsRead);
            return Json(new { notifications, unread });
        }
        [HttpPost]
        public IActionResult MarkNotificationRead(int id)
        {
            var n = _context.Notifications.Find(id);
            if (n != null) { n.IsRead = true; _context.SaveChanges(); }
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult MarkAllRead()
        {
            var notifications = _context.Notifications
                .Where(n => (n.TargetRole == "Admin" || n.TargetRole == "All") && !n.IsRead).ToList();
            notifications.ForEach(n => n.IsRead = true);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult SendNotification([FromBody] SendNotificationRequest request)
        {
            _context.Notifications.Add(new AppNotification
            {
                Title = request.Title,
                Message = request.Message,
                TargetRole = request.TargetRole,
                TargetClubId = request.TargetClubId,
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
            return Json(new { success = true });
        }
        public IActionResult ExportClubsExcel()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("الأندية");
            ws.Cell(1, 1).Value = "اسم النادي"; ws.Cell(1, 2).Value = "المسؤول"; ws.Cell(1, 3).Value = "المدينة";
            ws.Cell(1, 4).Value = "التصنيف"; ws.Cell(1, 5).Value = "الهاتف"; ws.Cell(1, 6).Value = "البريد";
            var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            for (int i = 0; i < clubs.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = clubs[i].Name; ws.Cell(i + 2, 2).Value = clubs[i].ManagerName;
                ws.Cell(i + 2, 3).Value = clubs[i].City; ws.Cell(i + 2, 4).Value = clubs[i].Category;
                ws.Cell(i + 2, 5).Value = clubs[i].Phone ?? ""; ws.Cell(i + 2, 6).Value = clubs[i].Email ?? "";
            }
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الأندية.xlsx");
        }
        public IActionResult ExportPlayersExcel()
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("اللاعبون");
            ws.Cell(1, 1).Value = "الاسم"; ws.Cell(1, 2).Value = "النادي"; ws.Cell(1, 3).Value = "الحزام";
            ws.Cell(1, 4).Value = "العمر"; ws.Cell(1, 5).Value = "الجنس"; ws.Cell(1, 6).Value = "الحالة الصحية";
            var players = _context.Players.ToList();
            for (int i = 0; i < players.Count; i++)
            {
                var club = _context.Clubs.FirstOrDefault(c => c.Id == players[i].ClubId);
                ws.Cell(i + 2, 1).Value = players[i].Name; ws.Cell(i + 2, 2).Value = club?.Name ?? "—";
                ws.Cell(i + 2, 3).Value = players[i].Belt ?? ""; ws.Cell(i + 2, 4).Value = players[i].Age;
                ws.Cell(i + 2, 5).Value = players[i].Gender ?? ""; ws.Cell(i + 2, 6).Value = players[i].HealthStatus ?? "";
            }
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "اللاعبون.xlsx");
        }
    }
    public class ResetPasswordRequest { public int ClubId { get; set; } public string NewPassword { get; set; } = ""; }
    public class ToggleMembershipRequest { public int MembershipId { get; set; } }
    public class TournamentRegistrationActionRequest { public int RegistrationId { get; set; } public string? Note { get; set; } }
    public class ToggleTournamentRequest { public int TournamentId { get; set; } }
    public class UpdateTournamentRequest { public int Id { get; set; } public string Title { get; set; } = ""; public DateTime Date { get; set; } public string City { get; set; } = ""; public string Location { get; set; } = ""; public string? Description { get; set; } public decimal RegistrationFee { get; set; } public int MaxPlayersPerClub { get; set; } public string? Categories { get; set; } }
    public class CheckEmailRequest { public string Email { get; set; } = ""; }
    public class CheckPhoneRequest { public string Phone { get; set; } = ""; }
    public class UpdateFeeSettingRequest { public decimal Fee { get; set; } }
    public class UpdateClubFeeRequest { public int MembershipId { get; set; } public decimal Fee { get; set; } }
    public class SendNotificationRequest { public string Title { get; set; } = ""; public string Message { get; set; } = ""; public string TargetRole { get; set; } = "All"; public int? TargetClubId { get; set; } }
}
