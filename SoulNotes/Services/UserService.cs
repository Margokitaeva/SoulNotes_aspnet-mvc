using Microsoft.Data.Sqlite;
using SoulNotes.Models;

namespace SoulNotes.Services
{
    public static class UserService
    {
        public static bool ValidateLogin(string provided_login, string provided_password)
        {
            using var connection = Db.OpenConnection();

            string hashedPassword = Db.ComputeMD5Hash(provided_password);
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM users WHERE login = $login AND password = $password;";
            cmd.Parameters.AddWithValue("$login", provided_login);
            cmd.Parameters.AddWithValue("$password", hashedPassword);

            long count = (long)cmd.ExecuteScalar();
            return count == 1;
        }

        public static int? GetUserId(string login)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id FROM users WHERE login = $login;";
            cmd.Parameters.AddWithValue("$login", login);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }

            return null;
        }

        public static int? GetUserIdByLogin(string login)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id FROM users WHERE login = $login";
            cmd.Parameters.AddWithValue("$login", login);

            var result = cmd.ExecuteScalar();
            if (result != null)
                return Convert.ToInt32(result);

            return null;
        }

        public static User GetUserByLogin(string login)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT id, login, password, token FROM users WHERE login = $login";
            cmd.Parameters.AddWithValue("$login", login);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    Token = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }

            return null;
        }

        public static bool IsUserAdmin(int userId)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT isAdmin FROM users WHERE id = $userId;";
            cmd.Parameters.AddWithValue("$userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetInt32(0) == 1;
            }

            return false;
        }

        public static void CreateUser(string login, string password)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO users (login, password, token) VALUES ($login, $password, $token);";
            cmd.Parameters.AddWithValue("$login", login);
            cmd.Parameters.AddWithValue("$password", Db.ComputeMD5Hash(password));
            string token = Guid.NewGuid().ToString();
            cmd.Parameters.AddWithValue("$token", token);
            cmd.ExecuteNonQuery();
        }

        public static List<string> GetAllUsers()
        {
            var users = new List<string>();

            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT login FROM users";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string login = reader.GetString(0);
                users.Add(login);
            }

            return users;
        }


    }
}