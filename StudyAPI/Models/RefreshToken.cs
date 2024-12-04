using System.ComponentModel.DataAnnotations;

namespace StudyAPI.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public  string JwtTokenId { get; set; }
        public string Refresh_Token { get; set; } //The prop name cannot match the class name
        public bool IsValid { get; set; } // Just valid for one use
        public DateTime ExpiresAt { get; set; }
    }
}
