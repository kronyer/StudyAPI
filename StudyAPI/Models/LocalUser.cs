namespace StudyAPI.Models
{
    public class LocalUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Password { get; set; } // nao será encriptado nesse projeto
        public string Role { get; set; }

    }
}
