using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SoulNotes.Services
{
    public static class Db
    {
        private static string connectionString = "Data Source = AppData.db";

        public static SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();

            return connection;
        }

        public static string ComputeMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}