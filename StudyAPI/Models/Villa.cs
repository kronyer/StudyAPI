using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyAPI.Models
{
    public class Villa
    {
        //Deixe os requireds para o DTO
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Isso é necessário? ou é redundante??
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public double Rate { get; set; }
        public int Occupancy { get; set; }
        public int Sqft { get; set; }
        public string ImageUrl { get; set; }
        public string Amenity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
