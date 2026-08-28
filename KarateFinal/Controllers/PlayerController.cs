using KarateFinal.Data;
using KarateFinal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KarateFinal.Controllers
{
    public class PlayerController : Controller
    {
        private readonly KarateContext _context;

        public PlayerController(KarateContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null)
                return RedirectToAction("Login", "Account");

            var player = _context.Players.Find(user.PlayerId.Value);
            if (player == null)
                return RedirectToAction("Login", "Account");

            var participations = _context.Participations
                .Where(p => p.ClubId == player.ClubId)
                .Include(p => p.Tournament)
                .ToList();

            ViewBag.Player = player;
            ViewBag.Participations = participations;
            ViewBag.TotalPoints = participations.Sum(p => p.Points);
            ViewBag.Gold = participations.Count(p => p.Rank == 1);
            ViewBag.Silver = participations.Count(p => p.Rank == 2);
            ViewBag.Bronze = participations.Count(p => p.Rank == 3);
            ViewBag.LastLogin = user?.LastLogin?.ToString("yyyy/MM/dd HH:mm") ?? "—";
            var pendingRequests = _context.TournamentPlayerRequests
    .Where(r => r.PlayerId == player.Id && r.Status == "بانتظار الموافقة")
    .Include(r => r.Tournament)
    .ToList();
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.InjuryRecords = _context.InjuryRecords
                .Where(r => r.PlayerId == player.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            ViewBag.PlayerMembership = _context.PlayerMemberships
                .FirstOrDefault(m => m.PlayerId == player.Id && m.Year == DateTime.Now.Year);
            return View();
        }

        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null) return RedirectToAction("Login", "Account");

            var player = _context.Players.Find(user.PlayerId.Value);
            ViewBag.Player = player;
            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        public IActionResult UpdateProfile(string newUsername, string newPassword)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null) return RedirectToAction("Login", "Account");

            var player = _context.Players.Find(user.PlayerId.Value);

            if (!string.IsNullOrEmpty(newUsername))
            {
                user.Username = newUsername;
                if (player != null) player.Username = newUsername;
                HttpContext.Session.SetString("Username", newUsername);
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                if (player != null) player.Password = newPassword;
            }

            _context.SaveChanges();
            TempData["Success"] = "تم تحديث المعلومات بنجاح ✅";
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public IActionResult RespondRequest([FromBody] RespondRequestModel request)
        {
            var req = _context.TournamentPlayerRequests.Find(request.RequestId);
            if (req == null) return Json(new { success = false });

            req.Status = request.Status;
            _context.SaveChanges();

            // لما اللاعب يوافق — سجّل النادي تلقائياً
            if (request.Status == "موافق")
            {
                var exists = _context.TournamentRegistrations
                    .Any(r => r.TournamentId == req.TournamentId && r.ClubId == req.ClubId);

                if (!exists)
                {
                    _context.TournamentRegistrations.Add(new TournamentRegistration
                    {
                        TournamentId = req.TournamentId,
                        ClubId = req.ClubId,
                        PlayersCount = 1,
                        RegisteredAt = DateTime.Now,
                        Status = "بانتظار الموافقة"
                    });
                }
                else
                {
                    // حدّث عدد اللاعبين
                    var reg = _context.TournamentRegistrations
                        .FirstOrDefault(r => r.TournamentId == req.TournamentId && r.ClubId == req.ClubId);
                    if (reg != null)
                    {
                        var approvedCount = _context.TournamentPlayerRequests
                            .Count(r => r.TournamentId == req.TournamentId
                                && r.ClubId == req.ClubId
                                && r.Status == "موافق");
                        reg.PlayersCount = approvedCount;
                    }
                }
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }
        public IActionResult Messages()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null) return RedirectToAction("Login", "Account");
            var player = _context.Players.Find(user.PlayerId.Value);
            var messages = _context.Messages
                .Where(m => m.SenderPlayerId == user.PlayerId.Value || m.ReceiverPlayerId == user.PlayerId.Value
                         || (m.ReceiverClubId == player.ClubId && m.SenderRole == "Club")
                         || (m.SenderClubId == player.ClubId && m.ReceiverRole == "All"))
                .OrderBy(m => m.SentAt)
                .ToList();
            ViewBag.Messages = messages;
            ViewBag.Player = player;
            return View();
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] PlayerSendMessageRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null) return Json(new { success = false });
            var player = _context.Players.Find(user.PlayerId.Value);
            _context.Messages.Add(new KarateFinal.Models.Message
            {
                SenderRole = "Player",
                SenderPlayerId = user.PlayerId.Value,
                ReceiverRole = "Club",
                ReceiverClubId = player?.ClubId,
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            });
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public class PlayerSendMessageRequest
        {
            public string Content { get; set; } = "";
        }
        public IActionResult Entitlements()
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null) return RedirectToAction("Login", "Account");
            var player = _context.Players.Find(user.PlayerId.Value);
            var memberships = _context.PlayerMemberships
                .Where(m => m.PlayerId == user.PlayerId.Value)
                .OrderByDescending(m => m.Year)
                .ToList();
            var receipts = _context.PaymentReceipts
                .Where(r => r.PlayerId == user.PlayerId.Value)
                .OrderByDescending(r => r.PaidDate)
                .ToList();
            ViewBag.Player = player;
            ViewBag.Memberships = memberships;
            ViewBag.Receipts = receipts;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddInjury([FromForm] PlayerInjuryRequest request)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user?.PlayerId == null) return Json(new { success = false });

            string? attachmentPath = null;
            if (request.Attachment != null && request.Attachment.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "injuries");
                Directory.CreateDirectory(uploadsDir);
                var fileName = Guid.NewGuid() + Path.GetExtension(request.Attachment.FileName);
                using var stream = new FileStream(Path.Combine(uploadsDir, fileName), FileMode.Create);
                await request.Attachment.CopyToAsync(stream);
                attachmentPath = "/uploads/injuries/" + fileName;
            }

            _context.InjuryRecords.Add(new InjuryRecord
            {
                PlayerId = user.PlayerId.Value,
                InjuryNote = request.InjuryNote,
                InjuryStart = request.InjuryStart,
                InjuryEnd = request.InjuryEnd,
                AttachmentPath = attachmentPath,
                CreatedAt = DateTime.Now
            });

            var player = _context.Players.Find(user.PlayerId.Value);
            if (player != null) player.HealthStatus = "مصاب";
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteInjury(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var injury = _context.InjuryRecords.Find(id);
            if (injury == null || injury.PlayerId != user?.PlayerId) return Json(new { success = false });
            _context.InjuryRecords.Remove(injury);
            var remaining = _context.InjuryRecords.Any(r => r.PlayerId == injury.PlayerId && r.Id != id);
            if (!remaining)
            {
                var player = _context.Players.Find(injury.PlayerId);
                if (player != null) player.HealthStatus = "سليم";
            }
            _context.SaveChanges();
            return Json(new { success = true });
        }

        public class RespondRequestModel
        {
            public int RequestId { get; set; }
            public string Status { get; set; } = "";
        }
    }
    public class PlayerInjuryRequest
    {
        public string InjuryNote { get; set; } = "";
        public DateTime InjuryStart { get; set; }
        public DateTime? InjuryEnd { get; set; }
        public IFormFile? Attachment { get; set; }
    }

}