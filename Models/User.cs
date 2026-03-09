namespace SoulNotes.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; } // ← ДОБАВЬ ЭТО
    }
}
