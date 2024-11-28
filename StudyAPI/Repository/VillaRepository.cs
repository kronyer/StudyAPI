using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;
using System.Linq.Expressions;

namespace StudyAPI.Repository
{
    public class VillaRepository : Repository<Villa>,  IVillaRepository
    {
        private readonly VillaDbContext _db;

        public VillaRepository(VillaDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Villa> UpdateAsync(Villa villa)
        {
            villa.UpdatedDate = DateTime.Now;
            _db.Villas.Update(villa);
            await _db.SaveChangesAsync();
            return villa;
        }
    }
}
