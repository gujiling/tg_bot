using Microsoft.EntityFrameworkCore;

namespace CoreTest
{
    public class SqliteContext : DbContext
    {
        public DbSet<StatisticInfo> Infos { get; set; }
        public DbSet<StatisticRule> Rules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./Statictis.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StatisticInfo>().HasKey(o => new { o.RuleId, o.UserId });
            // modelBuilder.Entity<StatisticInfo>().HasKey(o => new { o.RuleId, o.UserId });
            modelBuilder.Entity<StatisticRule>().HasKey(o => new { o.RuleId });
            // modelBuilder.Entity<StatisticRule>().HasKey(o => new { o.RuleId });
            base.OnModelCreating(modelBuilder);
        }
    }
}
