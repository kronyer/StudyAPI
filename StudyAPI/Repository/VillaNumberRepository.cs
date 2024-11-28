using Microsoft.EntityFrameworkCore;
using StudyAPI.Data;
using StudyAPI.Models;
using StudyAPI.Repository.IRepository;
using System.Linq.Expressions;

namespace StudyAPI.Repository
{
    public class VillaNumberRepository : Repository<VillaNumber>,  IVillaNumberRepository
    {
        private readonly VillaDbContext _db;

        public VillaNumberRepository(VillaDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<VillaNumber> UpdateAsync(VillaNumber villaNo)
        {
            villaNo.UpdatedDate = DateTime.Now;
            _db.VillaNumbers.Update(villaNo);
            await _db.SaveChangesAsync();
            return villaNo;
        }
    }
}
