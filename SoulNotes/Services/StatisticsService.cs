using SoulNotes.Models;
using Microsoft.Data.Sqlite;

namespace SoulNotes.Services
{
    public static class StatisticsService
    {
        public static int ExecuteCount(SqliteConnection conn, int userId, DateTime? start, DateTime? end)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM moodEntries WHERE userId = $uid";
            cmd.Parameters.AddWithValue("$uid", userId);
            if (start != null)
            {
                cmd.CommandText += " AND recordDate >= $start";
                cmd.Parameters.AddWithValue("$start", start.Value.ToString("yyyy-MM-dd"));
            }
            if (end != null)
            {
                cmd.CommandText += " AND recordDate <= $end";
                cmd.Parameters.AddWithValue("$end", end.Value.ToString("yyyy-MM-dd"));
            }

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static Dictionary<string, int> GetEmotionFrequency(SqliteConnection conn, int userId)
        {
            var result = new Dictionary<string, int>();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.name, COUNT(*) as count
                FROM emotionEntries ee
                JOIN emotions e ON ee.emotionId = e.emotionId
                JOIN moodEntries m ON ee.moodEntryId = m.moodEntryId
                WHERE m.userId = $uid
                GROUP BY e.name
                ORDER BY count DESC";
            cmd.Parameters.AddWithValue("$uid", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                int count = reader.GetInt32(1);
                result[name] = count;
            }

            return result;
        }

        public static string GetTopEmotion(SqliteConnection conn, int userId, DateTime from, DateTime to)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.name, COUNT(*) as count
                FROM emotionEntries ee
                JOIN emotions e ON ee.emotionId = e.emotionId
                JOIN moodEntries m ON ee.moodEntryId = m.moodEntryId
                WHERE m.userId = $uid AND m.recordDate BETWEEN $from AND $to
                GROUP BY e.name
                ORDER BY count DESC
                LIMIT 1";
            cmd.Parameters.AddWithValue("$uid", userId);
            cmd.Parameters.AddWithValue("$from", from.ToString("s"));
            cmd.Parameters.AddWithValue("$to", to.ToString("s"));

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : "-";
        }

        public static List<Emotion> GetTopEmotions(SqliteConnection conn, int userId, int limit)
        {
            var result = new List<Emotion>();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.emotionId, e.name, e.color
                FROM emotionEntries ee
                JOIN emotions e ON ee.emotionId = e.emotionId
                JOIN moodEntries m ON ee.moodEntryId = m.moodEntryId
                WHERE m.userId = $uid
                GROUP BY e.emotionId, e.name, e.color
                ORDER BY COUNT(*) DESC
                LIMIT $limit";
            cmd.Parameters.AddWithValue("$uid", userId);
            cmd.Parameters.AddWithValue("$limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Emotion
                {
                    EmotionId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Color = reader.GetString(2),
                    UserId = userId
                });
            }

            return result;
        }

        public static List<Tag> GetTopTags(SqliteConnection conn, int userId, int limit)
        {
            var result = new List<Tag>();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.tagId, t.name, t.userId, COUNT(*) as count
                FROM tagEntries te
                JOIN tags t ON te.tagId = t.tagId
                JOIN moodEntries m ON te.moodEntryId = m.moodEntryId
                WHERE m.userId = $uid
                GROUP BY t.tagId, t.name, t.userId
                ORDER BY count DESC
                LIMIT $limit";
            cmd.Parameters.AddWithValue("$uid", userId);
            cmd.Parameters.AddWithValue("$limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Tag
                {
                    TagId = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    UserId = reader.IsDBNull(2) ? -1 : reader.GetInt32(2)
                });
            }

            return result;
        }

        public static Dictionary<Emotion, List<Tag>> GetEmotionTagCorrelation(SqliteConnection conn, int userId)
        {
            var result = new Dictionary<Emotion, List<Tag>>();

            var allEmotions = EmotionService.GetAllEmotions(userId);
            var allTags = TagService.GetAllTags(userId);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT e.name AS emotionName, t.name AS tagName, COUNT(*) as count
                FROM moodEntries m
                JOIN emotionEntries ee ON m.moodEntryId = ee.moodEntryId
                JOIN tagEntries te ON m.moodEntryId = te.moodEntryId
                JOIN emotions e ON ee.emotionId = e.emotionId
                JOIN tags t ON te.tagId = t.tagId
                WHERE m.userId = $uid
                GROUP BY e.name, t.name
                ORDER BY e.name, count DESC";
            cmd.Parameters.AddWithValue("$uid", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string emotionName = reader.GetString(0);
                string tagName = reader.GetString(1);

                var emotion = allEmotions.FirstOrDefault(e => e.Name == emotionName);
                var tag = allTags.FirstOrDefault(t => t.Name == tagName);

                if (emotion == null || tag == null)
                    continue;

                if (!result.ContainsKey(emotion))
                    result[emotion] = new List<Tag>();

                if (!result[emotion].Any(t => t.TagId == tag.TagId))
                    result[emotion].Add(tag);
            }

            return result;
        }

        public static StatisticsModel GetStatistics(int userId)
        {
            var model = new StatisticsModel();
            using var connection = Db.OpenConnection();

            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            model.TotalRecords = ExecuteCount(connection, userId, null, null);
            model.RecordsToday = ExecuteCount(connection, userId, today, now);
            model.RecordsThisWeek = ExecuteCount(connection, userId, weekAgo, now);
            model.RecordsThisMonth = ExecuteCount(connection, userId, monthAgo, now);

            model.EmotionCounts = GetEmotionFrequency(connection, userId);

            model.TopEmotionWeek = GetTopEmotion(connection, userId, weekAgo, now);
            model.TopEmotionMonth = GetTopEmotion(connection, userId, monthAgo, now);

            model.TopEmotions = GetTopEmotions(connection, userId, 5);
            model.TopTags = GetTopTags(connection, userId, 5);

            model.EmotionTagCorrelation = GetEmotionTagCorrelation(connection, userId);

            return model;
        }

    }
}