using StudyAPI.Models;
using System.Linq.Expressions;

namespace StudyAPI.Repository.IRepository
{
    public interface IVillaRepository : IRepository<Villa>// Data Access Layer interage com a entidade Villa, nao com DTO
    {
        Task<Villa> UpdateAsync(Villa villa); // Logicas de update geralmente divergem
    }
}
