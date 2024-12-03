using Microsoft.AspNetCore.Identity;

namespace StudyAPI.Models
{
    public class VillaUser : IdentityUser
    {
        public string Name { get; set; }

    }
}
