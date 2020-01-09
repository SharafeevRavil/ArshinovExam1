using Microsoft.EntityFrameworkCore;

namespace ArshinovExam1.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Site> Sites { get; set; }
         
        public ApplicationDbContext()
        {
            Database.EnsureCreated();
        }
         
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=exam;Username=postgres;Password=postgres");
        }
    }

    public class Site
    {
        public int Id { get; set; }
        
        public string Domain { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
    }
}