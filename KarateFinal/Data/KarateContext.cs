using Microsoft.EntityFrameworkCore;
using KarateFinal.Models;
using System.Collections.Generic;

namespace KarateFinal.Data
{
    public class KarateContext : DbContext
    {
        public KarateContext(DbContextOptions<KarateContext> options)
        : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Participation> Participations { get; set; }
        public DbSet<PlayerMembership> PlayerMemberships { get; set; }
        public DbSet<TournamentRegistration> TournamentRegistrations { get; set; }
        public DbSet<TournamentPlayerRequest> TournamentPlayerRequests { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<PlayerResult> PlayerResults { get; set; }
        public DbSet<InjuryRecord> InjuryRecords { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
    }

}