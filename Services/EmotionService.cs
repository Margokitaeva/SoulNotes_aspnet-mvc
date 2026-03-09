using SoulNotes.Models;

namespace SoulNotes.Services
{
    public static class EmotionService
    {
        public static List<Emotion> GetAllEmotions(int userId)
        {
            var emotions = new List<Emotion>();
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT emotionId, name, color FROM emotions WHERE userId IS NULL OR userId = $userId;";
            cmd.Parameters.AddWithValue("$userId", userId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                emotions.Add(new Emotion
                {
                    EmotionId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Color = reader.GetString(2),
                    UserId = userId
                });
            }

            return emotions;
        }

        public static void AddEmotion(string name, string color, int userId)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO emotions (name, color, userId) VALUES ($name, $color, $userId);";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$color", color);
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteEmotion(int emotionId, int userId)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM emotionEntries WHERE emotionId = $emotionId";
            cmd.Parameters.AddWithValue("$emotionId", emotionId);
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();


            cmd.CommandText = "DELETE FROM emotions WHERE emotionId = $emotionId AND userId = $userId";
            cmd.Parameters.AddWithValue("$emotionId", emotionId);
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}