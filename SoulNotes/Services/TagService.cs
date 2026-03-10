using SoulNotes.Models;

namespace SoulNotes.Services
{
    public static class TagService
    {
        public static List<Tag> GetAllTags(int userId)
        {
            var tags = new List<Tag>();
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT tagId, name, userId FROM tags WHERE userId IS NULL OR userId = $userId;";
            cmd.Parameters.AddWithValue("$userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(new Tag
                {
                    TagId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    UserId = userId
                });
            }

            return tags;
        }

        public static void AddTag(string name, int userId)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO tags (name, userId) VALUES ($name, $userId);";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteTag(int tagId, int userId)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM tagEntries WHERE tagid = $tagId";
            cmd.Parameters.AddWithValue("$tagId", tagId);
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

            cmd.CommandText = "DELETE FROM tags WHERE tagId = $tagId AND userId = $userId";
            cmd.Parameters.AddWithValue("$tagId", tagId);
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.ExecuteNonQuery();
        }


    }
}