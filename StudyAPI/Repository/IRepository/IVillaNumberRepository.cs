using StudyAPI.Models;
using System.Linq.Expressions;

namespace StudyAPI.Repository.IRepository
{
    public interface IVillaNumberRepository : IRepository<VillaNumber>// Data Access Layer interage com a entidade Villa, nao com DTO
    {
        Task<VillaNumber> UpdateAsync(VillaNumber villaNo); // Logicas de update geralmente divergem
    }
}
