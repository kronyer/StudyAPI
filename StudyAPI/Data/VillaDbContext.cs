using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyAPI.Models;

namespace StudyAPI.Data
{
    public class VillaDbContext : IdentityDbContext<VillaUser>
    {
        public VillaDbContext(DbContextOptions<VillaDbContext> options) : base(options)
        {
        }

        public DbSet<VillaUser> VillaUsers;
        public DbSet<LocalUser  > Users{ get; set; } 
        public DbSet<Villa> Villas { get; set; } //Sera o nome da tabela
        public DbSet<VillaNumber> VillaNumbers { get; set; } //Sera o nome da tabela

        protected override void OnModelCreating(ModelBuilder modelBuilder) //para inset no db
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Villa>().HasData(new Villa()
            {
                Id = 1,
                Name = "Villa 1",
                Details = "Villa 1 Details",
                Rate = 100,
                Occupancy = 4,
                Sqft = 2000,
                ImageUrl = "https://via.placeholder.com/150",
                Amenity = "Pool",
            });
        }
    }
}
