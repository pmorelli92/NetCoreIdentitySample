namespace NetCore.Identity.Sample.API.Models
{
    public class UserModel
    {
        public string Email { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public UserModel(
            string email,
            string username,
            string password)
        {
            Email = email;
            Username = username;
            Password = password;
        }
    }
}