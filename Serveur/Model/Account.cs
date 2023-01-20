namespace Server.Model
{
    public class Account
    {
        public String Username { get; set; }
        public String Email { get; set; }
        public bool IsValidated { get; set; }

        public Account(String username, String email, bool isValidated)
        {
            this.Username = username;
            this.Email = email;
            this.IsValidated = isValidated;
        }
    }
}
