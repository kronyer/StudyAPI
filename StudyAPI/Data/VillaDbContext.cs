using Microsoft.EntityFrameworkCore;
using StudyAPI.Models;

namespace StudyAPI.Data
{
    public class VillaDbContext : DbContext
    {
        public VillaDbContext(DbContextOptions<VillaDbContext> options) : base(options)
        {
        }
        public DbSet<Villa> Villas { get; set; } //Sera o nome da tabela
    }
}
