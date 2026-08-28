using KarateFinal.Data;
using KarateFinal.Models;
using KarateFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarateFinal.Controllers
{
    public class ClubController : Controller
    {
        private readonly KarateContext _context;
        private readonly EmailService _emailService;

        public ClubController(KarateContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            var clubs = _context.Clubs.Where(c => !c.IsDeleted).ToList();
            ViewBag.Clubs = clubs;
            return View();
        }
        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var players = _context.Players.Where(p => p.ClubId == user.ClubId).ToList();
            var club = _context.Clubs.Find(user.ClubId);
            ViewBag.Username = username;
            ViewBag.Players = players;
            ViewBag.PlayersCount = players.Count;
            ViewBag.Tournaments = _context.Tournaments.ToList();
            ViewBag.LogoImage = club?.LogoImage;
            ViewBag.MaleImage = club?.MaleImage;
            ViewBag.FemaleImage = club?.FemaleImage;
            ViewBag.LastLogin = user?.LastLogin?.ToString("yyyy/MM/dd HH:mm") ?? "---";
            return View();
        }
        [HttpPost]
        public IActionResult RequestNationalTeam([FromBody] NationalTeamRequest request)
        {
            var player = _context.Players.Find(request.PlayerId);
            if (player == null) return Json(new { success = false });
            player.NationalTeamStatus = "بانتظار الموافقة";
            _context.SaveChanges();
            // إشعار للأدمن
            var notification = new KarateFinal.Models.AppNotification
            {
                Title = "طلب انضمام للمنتخب",
                Message = $"طلب النادي إضافة اللاعب {player.Name} لقائمة المنتخب",
                TargetRole = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult AddPaymentReceipt([FromBody] AddReceiptRequest request)
        {
            try
            {
                var username = HttpContext.Session.GetString("Username");
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                if (user?.ClubId == null) return Json(new { success = false, error = "no club" });
                var player = _context.Players.Find(request.PlayerId);
                if (player == null) return Json(new { success = false, error = "no player" });
                var membership = _context.PlayerMemberships
                    .FirstOrDefault(m => m.PlayerId == request.PlayerId && m.Year == request.Year);
                if (membership == null)
                {
                    membership = new PlayerMembership
                    {
                        PlayerId = request.PlayerId,
                        ClubId = user.ClubId.Value,
                        Year = request.Year,
                        MonthlyFee = request.Amount,
                        PaidMonths = request.Month.ToString()
                    };
                    _context.PlayerMemberships.Add(membership);
                }
                else
                {
                    var paidList = (membership.PaidMonths ?? "").Split(',').Where(p => !string.IsNullOrEmpty(p)).ToList();
                    if (!paidList.Contains(request.Month.ToString()))
                        paidList.Add(request.Month.ToString());
                    membership.PaidMonths = string.Join(",", paidList);
                }
                var receipt = new KarateFinal.Models.PaymentReceipt
                {
                    PlayerId = request.PlayerId,
                    ClubId = user.ClubId.Value,
                    Year = request.Year,
                    Month = request.Month,
                    Amount = request.Amount,
                    PaidDate = DateTime.UtcNow,
                    Notes = request.Notes,
                    CreatedBy = username ?? ""
                };
                _context.PaymentReceipts.Add(receipt);
                _context.SaveChanges();
                return Json(new { success = true, receiptId = receipt.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        public class AddReceiptRequest
        {
            public int PlayerId { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
        }
        public class NationalTeamRequest
        {
            public int PlayerId { get; set; }
        }
        public IActionResult AddPlayer()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("Login", "Account");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            ViewBag.ClubId = user?.ClubId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer(Player player)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId != null) player.ClubId = user.ClubId.Value;
          
            _context.Players.Add(player);
            _context.SaveChanges();
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var rng = new Random();
            var plainPassword = "Kar@" + new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
            var playerUsername = "player" + player.Id;
            player.Username = playerUsername;
            player.Password = plainPassword;
            _context.Users.Add(new User { Username = playerUsername, Password = BCrypt.Net.BCrypt.HashPassword(plainPassword), Role = "Player", ClubId = player.ClubId, PlayerId = player.Id, MustChangePassword = true });
            _context.SaveChanges();
            if (!string.IsNullOrEmpty(player.Email))
            {
                var emailTo = player.Email;
                var emailName = player.Name;
                var emailSubject = "بيانات دخولك إلى منصة الكاراتيه الفلسطينية";
                var emailBody = "<div dir='rtl' style='font-family:Arial;padding:20px;'><h2 style='color:#1e2a38;'>مرحباً " + player.Name + " 🥋</h2><p>تم تسجيلك في منصة الكاراتيه الفلسطينية.</p><div style='background:#f8fafc;padding:16px;border-radius:8px;border:1px solid #e2e8f0;margin:16px 0;'><p><strong>اسم المستخدم:</strong> " + playerUsername + "</p><p><strong>كلمة المرور:</strong> " + plainPassword + "</p></div><p style='color:#888;font-size:12px;'>يرجى تغيير كلمة المرور عند أول تسجيل دخول.</p></div>";
                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendAsync(emailTo, emailName, emailSubject, emailBody); }
                    catch (Exception ex) { Console.WriteLine("Email error: " + ex.Message); }
                });
            }
            TempData["NewPlayerUsername"] = playerUsername;
            TempData["NewPlayerPassword"] = plainPassword;
            TempData["NewPlayerName"] = player.Name;
            return RedirectToAction("PlayerCreated");
        }

        public IActionResult PlayerCreated()
        {
            ViewBag.PlayerUsername = TempData["NewPlayerUsername"];
            ViewBag.PlayerPassword = TempData["NewPlayerPassword"];
            ViewBag.PlayerName = TempData["NewPlayerName"];
            return View();
        }

        public IActionResult Players()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            ViewBag.Players = _context.Players.Where(p => p.ClubId == user.ClubId).ToList();
            return View();
        }

        public IActionResult Best()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            var players = _context.Players.Where(p => p.ClubId == user.ClubId.Value).ToList().OrderByDescending(p => p.Age).ToList();
            ViewBag.Players = players;
            return View();
        }

        public IActionResult Participation()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            var participations = _context.Participations.Where(p => p.ClubId == user.ClubId.Value).Include(p => p.Tournament).ToList();
            ViewBag.Participations = participations;
            ViewBag.Total = participations.Count;
            ViewBag.Gold = participations.Count(p => p.Rank == 1);
            ViewBag.TotalPoints = participations.Sum(p => p.Points);
            return View();
        }
        public IActionResult ReceiptTemplate()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            var club = _context.Clubs.Find(user.ClubId.Value);
            ViewBag.Club = club;
            return View();
        }

        [HttpPost]
        public IActionResult SaveReceiptTemplate([FromBody] ReceiptTemplateRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });
            var club = _context.Clubs.Find(user.ClubId.Value);
            if (club == null) return Json(new { success = false });
            club.ReceiptTemplate = request.Template;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public class ReceiptTemplateRequest
        {
            public string Template { get; set; } = "";
        }
        public IActionResult Entitlements(int? year)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            int clubId = user.ClubId.Value;
            int selectedYear = year ?? DateTime.Now.Year;
            var players = _context.Players.Where(p => p.ClubId == clubId).ToList();
            foreach (var p in players)
            {
                var exists = _context.PlayerMemberships.Any(m => m.PlayerId == p.Id && m.Year == selectedYear);
                if (!exists)
                {
                    _context.PlayerMemberships.Add(new PlayerMembership { PlayerId = p.Id, ClubId = clubId, Year = selectedYear, MonthlyFee = 150, OldDebt = 0, PaidMonths = "" });
                }
            }
            _context.SaveChanges();
            var memberships = _context.PlayerMemberships.Where(m => m.ClubId == clubId && m.Year == selectedYear).Include(m => m.Player).ToList();
            ViewBag.Memberships = memberships;
            ViewBag.Year = selectedYear;
            return View();
        }

        [HttpPost]
        public IActionResult ToggleMonth([FromBody] ToggleMonthRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var membership = _context.PlayerMemberships.Find(request.MembershipId);
            if (membership == null) return Json(new { success = false });

            var paid = membership.PaidMonths.Split(',').Where(x => x != "").ToList();
            bool isNowPaid = !paid.Contains(request.Month.ToString());

            if (paid.Contains(request.Month.ToString()))
            {
                paid.Remove(request.Month.ToString());
                // حذف الوصل
                var receipt = _context.PaymentReceipts.FirstOrDefault(r => r.PlayerId == membership.PlayerId && r.Year == membership.Year && r.Month == request.Month);
                if (receipt != null) _context.PaymentReceipts.Remove(receipt);
            }
            else
            {
                paid.Add(request.Month.ToString());
                // إضافة وصل تلقائي
                var exists = _context.PaymentReceipts.Any(r => r.PlayerId == membership.PlayerId && r.Year == membership.Year && r.Month == request.Month);
                if (!exists)
                {
                    _context.PaymentReceipts.Add(new KarateFinal.Models.PaymentReceipt
                    {
                        PlayerId = membership.PlayerId,
                        ClubId = membership.ClubId,
                        Year = membership.Year,
                        Month = request.Month,
                        Amount = membership.MonthlyFee,
                        PaidDate = DateTime.UtcNow,
                        CreatedBy = username ?? ""
                    });
                }
            }

            membership.PaidMonths = string.Join(",", paid.OrderBy(x => int.Parse(x)));
            _context.SaveChanges();
            return Json(new { success = true, paidMonths = membership.PaidMonths });
        }

        [HttpPost]
        public IActionResult UpdateFee([FromBody] UpdateFeeRequest request)
        {
            var membership = _context.PlayerMemberships.Find(request.MembershipId);
            if (membership == null) return Json(new { success = false });
            membership.MonthlyFee = request.MonthlyFee;
            membership.OldDebt = request.OldDebt;
            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult PayMonth([FromBody] PayMonthRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });

            var membership = _context.PlayerMemberships.Find(request.MembershipId);
            if (membership == null) return Json(new { success = false });

            // تحديث الأشهر المدفوعة
            var paidList = (membership.PaidMonths ?? "").Split(',').Where(p => !string.IsNullOrEmpty(p)).ToList();
            if (!paidList.Contains(request.Month.ToString()))
                paidList.Add(request.Month.ToString());
            membership.PaidMonths = string.Join(",", paidList);

            // إضافة وصل دفع تلقائي
            var existingReceipt = _context.PaymentReceipts
                .FirstOrDefault(r => r.PlayerId == membership.PlayerId && r.Year == membership.Year && r.Month == request.Month);

            if (existingReceipt == null)
            {
                _context.PaymentReceipts.Add(new KarateFinal.Models.PaymentReceipt
                {
                    PlayerId = membership.PlayerId,
                    ClubId = user.ClubId.Value,
                    Year = membership.Year,
                    Month = request.Month,
                    Amount = membership.MonthlyFee,
                    PaidDate = DateTime.UtcNow,
                    CreatedBy = username ?? ""
                });
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        public class PayMonthRequest
        {
            public int MembershipId { get; set; }
            public int Month { get; set; }
        }
        [HttpPost]
        public IActionResult PayOldDebt([FromBody] PayOldDebtRequest request)
        {
            var membership = _context.PlayerMemberships.Find(request.MembershipId);
            if (membership == null) return Json(new { success = false, message = "لم يتم العثور على السجل" });
            if (request.Amount <= 0) return Json(new { success = false, message = "المبلغ يجب أن يكون أكبر من 0" });
            if (request.Amount > membership.OldDebt) return Json(new { success = false, message = "المبلغ أكبر من الاستحقاق القديم" });
            membership.OldDebt -= request.Amount;
            _context.SaveChanges();
            return Json(new { success = true, newOldDebt = membership.OldDebt });
        }

        public IActionResult PlayerCard(int id)
        {
            var player = _context.Players.Find(id);
            if (player == null) return RedirectToAction("Best");

            var participations = _context.Participations.Where(p => p.ClubId == player.ClubId).Include(p => p.Tournament).ToList();
            var playerResults = _context.PlayerResults.Where(r => r.PlayerId == id).ToList();

            ViewBag.Player = player;
            ViewBag.Participations = participations;
            ViewBag.PlayerResults = playerResults;
            ViewBag.InjuryRecords = _context.InjuryRecords.Where(r => r.PlayerId == id).OrderByDescending(r => r.CreatedAt).ToList();
            ViewBag.TotalPoints = playerResults.Sum(r => r.Points);
            ViewBag.Gold = playerResults.Count(r => r.Rank == 1);
            ViewBag.Silver = playerResults.Count(r => r.Rank == 2);
            ViewBag.Bronze = playerResults.Count(r => r.Rank == 3);
            ViewBag.PlayerTournaments = _context.TournamentPlayerRequests
                .Where(r => r.PlayerId == id && r.Status == "موافق")
                .Include(r => r.Tournament)
                .Select(r => new { r.TournamentId, r.Tournament.Title })
                .ToList();

            try
            {
                ViewBag.Receipts = _context.PaymentReceipts
                    .Where(r => r.PlayerId == id)
                    .OrderByDescending(r => r.PaidDate)
                    .ToList();
            }
            catch { ViewBag.Receipts = null; }

            return View();
        }

        [HttpPost]
        public IActionResult ResetPlayerPassword([FromBody] ResetPlayerPasswordRequest request)
        {
            var player = _context.Players.Find(request.PlayerId);
            var user = _context.Users.FirstOrDefault(u => u.PlayerId == request.PlayerId);
            if (player == null || user == null) return Json(new { success = false });
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            var rng = new Random();
            var newPassword = "Kar@" + new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
            player.Password = newPassword;
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.MustChangePassword = true;
            _context.SaveChanges();
            if (!string.IsNullOrEmpty(player.Email))
            {
                var emailTo = player.Email;
                var emailName = player.Name;
                var emailSubject = "تم إعادة تعيين كلمة المرور — منصة الكاراتيه الفلسطينية";
                var emailBody = "<div dir='rtl' style='font-family:Arial;padding:20px;'><h2 style='color:#1e2a38;'>مرحباً " + player.Name + " 🥋</h2><p>تم إعادة تعيين كلمة مرورك.</p><div style='background:#f8fafc;padding:16px;border-radius:8px;border:1px solid #e2e8f0;margin:16px 0;'><p><strong>اسم المستخدم:</strong> " + player.Username + "</p><p><strong>كلمة المرور الجديدة:</strong> " + newPassword + "</p></div><p style='color:#888;font-size:12px;'>يرجى تغيير كلمة المرور عند أول تسجيل دخول.</p></div>";
                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendAsync(emailTo, emailName, emailSubject, emailBody); }
                    catch (Exception ex) { Console.WriteLine("Email error: " + ex.Message); }
                });
            }
            return Json(new { success = true, newPassword });
        }

        [HttpPost]
        public IActionResult SaveImages([FromBody] SaveImagesRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });
            var club = _context.Clubs.Find(user.ClubId.Value);
            if (club == null) return Json(new { success = false });
            if (request.LogoImage != null) club.LogoImage = request.LogoImage;
            if (request.MaleImage != null) club.MaleImage = request.MaleImage;
            if (request.FemaleImage != null) club.FemaleImage = request.FemaleImage;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult DeletePlayer(int id)
        {
            var player = _context.Players.Find(id);
            if (player != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.PlayerId == id);
                if (user != null) _context.Users.Remove(user);
                _context.Players.Remove(player);
                _context.SaveChanges();
            }
            return RedirectToAction("Best");
        }

        public IActionResult Tournaments()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            int clubId = user.ClubId.Value;
            ViewBag.Tournaments = _context.Tournaments.ToList();
            ViewBag.MyRegistrations = _context.TournamentRegistrations.Where(r => r.ClubId == clubId).ToList();
            ViewBag.ClubId = clubId;
            ViewBag.ClubPlayers = _context.Players.Where(p => p.ClubId == clubId).ToList();
            ViewBag.MyRequests = _context.TournamentPlayerRequests.Where(r => r.ClubId == clubId).Include(r => r.Player).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult RegisterTournament([FromBody] RegisterTournamentSimpleRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });
            var membership = _context.Memberships.FirstOrDefault(m => m.ClubId == user.ClubId.Value && m.Year == DateTime.Now.Year);
            if (membership == null || membership.Status == "غير مدفوع")
                return Json(new { success = false, message = "يجب دفع رسوم العضوية السنوية أولاً" });
            var hasPlayers = _context.Players.Any(p => p.ClubId == user.ClubId.Value);
            if (!hasPlayers) return Json(new { success = false, message = "لا يمكنك التسجيل لعدم وجود لاعبين" });
            var exists = _context.TournamentRegistrations.Any(r => r.TournamentId == request.TournamentId && r.ClubId == user.ClubId.Value);
            if (exists) return Json(new { success = false, message = "أنت مسجّل في هذه البطولة مسبقاً" });
            var approvedCount = _context.TournamentPlayerRequests.Count(r => r.TournamentId == request.TournamentId && r.ClubId == user.ClubId.Value && r.Status == "موافق");
            _context.TournamentRegistrations.Add(new TournamentRegistration { TournamentId = request.TournamentId, ClubId = user.ClubId.Value, PlayersCount = approvedCount, RegisteredAt = DateTime.Now, Status = "موافق" });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdatePlayerCount([FromBody] UpdatePlayerCountRequest request)
        {
            var reg = _context.TournamentRegistrations.FirstOrDefault(r => r.TournamentId == request.TournamentId && r.ClubId == request.ClubId);
            if (reg == null) return Json(new { success = false });
            reg.PlayersCount = _context.TournamentPlayerRequests.Count(r => r.TournamentId == request.TournamentId && r.ClubId == request.ClubId && r.Status == "موافق");
            _context.SaveChanges();
            return Json(new { success = true, count = reg.PlayersCount });
        }

        [HttpPost]
        public IActionResult NewYear()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });
            int clubId = user.ClubId.Value;
            int newYear = DateTime.Now.Year + 1;
            var players = _context.Players.Where(p => p.ClubId == clubId).ToList();
            foreach (var p in players)
            {
                var currentMembership = _context.PlayerMemberships.FirstOrDefault(m => m.PlayerId == p.Id && m.Year == DateTime.Now.Year);
                decimal oldDebt = 0;
                if (currentMembership != null)
                {
                    var unpaidMonths = 12 - currentMembership.PaidMonths.Split(',').Where(x => x != "").Count();
                    oldDebt = unpaidMonths > 0 ? (unpaidMonths * currentMembership.MonthlyFee) + currentMembership.OldDebt : 0;
                }
                var newMembership = _context.PlayerMemberships.FirstOrDefault(m => m.PlayerId == p.Id && m.Year == newYear);
                if (newMembership == null)
                    _context.PlayerMemberships.Add(new PlayerMembership { PlayerId = p.Id, ClubId = clubId, Year = newYear, MonthlyFee = currentMembership?.MonthlyFee ?? 150, OldDebt = oldDebt, PaidMonths = "" });
                else { newMembership.PaidMonths = ""; newMembership.OldDebt = oldDebt; }
            }
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            ViewBag.Club = _context.Clubs.Find(user.ClubId.Value);
            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string name, string email, string phone, string description, string newPassword, string newUsername)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            var club = _context.Clubs.Find(user.ClubId.Value);
            if (club != null)
            {
                if (!string.IsNullOrEmpty(name)) club.Name = name;
                if (!string.IsNullOrEmpty(email)) club.Email = email;
                if (!string.IsNullOrEmpty(phone)) club.Phone = phone;
                if (!string.IsNullOrEmpty(description)) club.Description = description;
            }
            if (!string.IsNullOrEmpty(newUsername)) { user.Username = newUsername; if (club != null) club.Username = newUsername; HttpContext.Session.SetString("Username", newUsername); }
            if (!string.IsNullOrEmpty(newPassword)) { user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); if (club != null) club.Password = newPassword; }
            _context.SaveChanges();
            TempData["Success"] = "تم تحديث المعلومات بنجاح";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult AddPlayerResult([FromBody] AddPlayerResultRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });
            _context.PlayerResults.Add(new PlayerResult { PlayerId = request.PlayerId, ClubId = user.ClubId.Value, TournamentName = request.TournamentName, Rank = request.Rank, Points = request.Points, Date = DateTime.Now });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult DeletePlayerResult(int id)
        {
            var result = _context.PlayerResults.Find(id);
            if (result != null) { _context.PlayerResults.Remove(result); _context.SaveChanges(); }
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult RequestPlayerTournament([FromBody] PlayerTournamentRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return Json(new { success = false });
            var playerCheck = _context.Players.Find(request.PlayerId);
            if (playerCheck?.HealthStatus == "مصاب") return Json(new { success = false, message = "⚠️ لا يمكن تسجيل اللاعب — حالته الصحية: مصاب" });
            if (playerCheck?.PlayerStatus == "موقوف") return Json(new { success = false, message = "⚠️ لا يمكن تسجيل اللاعب — اللاعب موقوف حالياً" });
            var exists = _context.TournamentPlayerRequests.Any(r => r.PlayerId == request.PlayerId && r.TournamentId == request.TournamentId);
            if (exists) return Json(new { success = false, message = "تم إرسال الطلب مسبقاً" });
            _context.TournamentPlayerRequests.Add(new TournamentPlayerRequest { TournamentId = request.TournamentId, PlayerId = request.PlayerId, ClubId = user.ClubId.Value, Status = "بانتظار الموافقة" });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdatePlayerStatus([FromBody] UpdatePlayerStatusRequest request)
        {
            var player = _context.Players.Find(request.PlayerId);
            if (player == null) return Json(new { success = false });
            player.PlayerStatus = request.PlayerStatus;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddInjury([FromForm] AddInjuryRequest request)
        {
            var player = _context.Players.Find(request.PlayerId);
            if (player == null) return Json(new { success = false });
            string? attachmentPath = null;
            if (request.Attachment != null && request.Attachment.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "injuries");
                Directory.CreateDirectory(uploadsDir);
                var fileName = Guid.NewGuid() + Path.GetExtension(request.Attachment.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await request.Attachment.CopyToAsync(stream);
                attachmentPath = "/uploads/injuries/" + fileName;
            }
            _context.InjuryRecords.Add(new InjuryRecord { PlayerId = request.PlayerId, InjuryNote = request.InjuryNote, InjuryStart = request.InjuryStart, InjuryEnd = request.InjuryEnd, AttachmentPath = attachmentPath, CreatedAt = DateTime.Now });
            player.HealthStatus = "مصاب";
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteInjury(int id)
        {
            var injury = _context.InjuryRecords.Find(id);
            if (injury == null) return Json(new { success = false });
            _context.InjuryRecords.Remove(injury);
            var remaining = _context.InjuryRecords.Any(r => r.PlayerId == injury.PlayerId && r.Id != id);
            if (!remaining) { var player = _context.Players.Find(injury.PlayerId); if (player != null) player.HealthStatus = "سليم"; }
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult CheckPlayerEmail([FromBody] CheckPlayerEmailRequest request)
        {
            var exists = _context.Players.Any(p => p.Email == request.Email) || _context.Clubs.Any(c => c.Email == request.Email);
            return Json(new { exists });
        }

        [HttpPost]
        public IActionResult UpdatePlayerNotes([FromBody] UpdateNotesRequest request)
        {
            var player = _context.Players.Find(request.PlayerId);
            if (player == null) return Json(new { success = false });
            player.Notes = request.Notes;
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public IActionResult PrintPlayers()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.ClubId == null) return RedirectToAction("Login", "Account");
            var club = _context.Clubs.Find(user.ClubId.Value);
            var players = _context.Players.Where(p => p.ClubId == user.ClubId.Value).ToList();
            ViewBag.Club = club;
            ViewBag.Players = players;
            return View();
        }
    }

    public class AddPlayerResultRequest { public int PlayerId { get; set; } public string TournamentName { get; set; } = ""; public int Rank { get; set; } public int Points { get; set; } }
    public class PlayerTournamentRequest { public int TournamentId { get; set; } public int PlayerId { get; set; } }
    public class SaveImagesRequest { public string? LogoImage { get; set; } public string? MaleImage { get; set; } public string? FemaleImage { get; set; } }
    public class ToggleMonthRequest { public int MembershipId { get; set; } public int Month { get; set; } }
    public class UpdateFeeRequest { public int MembershipId { get; set; } public decimal MonthlyFee { get; set; } public decimal OldDebt { get; set; } }
    public class RegisterTournamentSimpleRequest { public int TournamentId { get; set; } }
    public class UpdatePlayerCountRequest { public int TournamentId { get; set; } public int ClubId { get; set; } }
    public class PayOldDebtRequest { public int MembershipId { get; set; } public decimal Amount { get; set; } }
    public class ResetPlayerPasswordRequest { public int PlayerId { get; set; } }
    public class UpdatePlayerStatusRequest { public int PlayerId { get; set; } public string PlayerStatus { get; set; } = "ملتزم"; }
    public class UpdateNotesRequest { public int PlayerId { get; set; } public string? Notes { get; set; } }
    public class CheckPlayerEmailRequest { public string Email { get; set; } = ""; }
    public class AddInjuryRequest { public int PlayerId { get; set; } public string InjuryNote { get; set; } = ""; public DateTime InjuryStart { get; set; } public DateTime? InjuryEnd { get; set; } public IFormFile? Attachment { get; set; } }
}