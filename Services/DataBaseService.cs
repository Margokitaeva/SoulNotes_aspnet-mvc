using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Data.Sqlite;
using SoulNotes.Models;
using SQLitePCL;

public static class DataBaseService
{
    private static string connectionString = "Data Source = AppData.db";

    public static void InitializeDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            login TEXT NOT NULL,
            password TEXT NOT NULL,
            isAdmin INTEGER DEFAULT 0, 
            token TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS emotions (
            emotionId INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            color TEXT NOT NULL,
            userId INTEGER,
            FOREIGN KEY (userId) REFERENCES users (id)
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS tags (
            tagId INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            userId INTEGER
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS moodEntries (
            moodEntryId INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            userId INT NOT NULL ,
            recordDate TEXT NOT NULL,
            description TEXT,
            primaryEmotionId INTEGER NOT NULL,
            FOREIGN KEY (userId) REFERENCES users(id),
            FOREIGN KEY (primaryEmotionId) REFERENCES emotions (emotionId)
            );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS emotionEntries (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            moodEntryId INTEGER NOT NULL,
            emotionId INTEGER NOT NULL,
            FOREIGN KEY (moodEntryId) REFERENCES moodEntries (moodEntryId),
            FOREIGN KEY (emotionId) REFERENCES emotions (emotionId)
        );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS tagEntries (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            moodEntryId INTEGER NOT NULL,
            tagId INTEGER NOT NULL,
            FOREIGN KEY (tagId) REFERENCES tags (tagId),
            FOREIGN KEY (moodEntryId) REFERENCES moodEntries (moodEntryId)
        );";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT COUNT(*) FROM users;";
        long count = (long)cmd.ExecuteScalar();
        if (count == 0)
        {
            string hashedPassword = ComputeMD5Hash("Mercy");
            cmd.CommandText = "INSERT INTO users (login, password, token) VALUES ('Playlist', $password, 'playlist');";
            cmd.Parameters.AddWithValue("$password", hashedPassword);
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

            cmd.CommandText = "INSERT INTO users (login, password, isAdmin, token) VALUES ('admin', $password, 1, 'admin');";
            cmd.Parameters.AddWithValue("$password", ComputeMD5Hash("admin"));
            cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();

            cmd.CommandText = @"
            INSERT INTO emotions (name, color, userId) VALUES
                ('anxiety', '#ff7a04', NULL),
                ('envy', '#00d5c0', NULL),
                ('embarrasment', '#fc65ac', NULL),
                ('boredom', '#7983ff', NULL),
                ('disgust', '#8ae501', NULL),
                ('nostalgia', '#ca7aff', NULL),
                ('happiness', '#facb01', NULL),
                ('joy', '#fafd7a', NULL),
                ('anger', '#ff4d50', NULL),
                ('sadness', '#5194ff', NULL)
            ;";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
            INSERT INTO tags (name, userId) VALUES
                ('work', NULL),
                ('school', NULL),
                ('university', NULL),
                ('study', NULL),
                ('self-development', NULL),
                ('family', NULL),
                ('parents', NULL),
                ('siblings', NULL),
                ('friends', NULL),
                ('routine', NULL),
                ('hobbies', NULL),
                ('sport', NULL);";
            cmd.ExecuteNonQuery();
        }
    }

    public static bool ValidateLogin(string provided_login, string provided_password)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string hashedPassword = ComputeMD5Hash(provided_password);
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM users WHERE login = $login AND password = $password;";
        cmd.Parameters.AddWithValue("$login", provided_login);
        cmd.Parameters.AddWithValue("$password", hashedPassword);

        long count = (long)cmd.ExecuteScalar();
        return count == 1;

    }

    private static string ComputeMD5Hash(string input)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }


    public static int? GetUserId(string login)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static RecordFormModel GetFormData(int formId, int userId)
    {
        /*
        moodEntryId INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            userId INT NOT NULL ,
            date TEXT NOT NULL,
            description TEXT,
            primaryEmotionId
            */
        if (formId == -1)
            return null;
        RecordFormModel form = new RecordFormModel();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd1 = connection.CreateCommand();
        cmd1.CommandText = "SELECT name, recordDate, description, primaryEmotionId FROM moodEntries WHERE userId = $userId AND moodEntryId = $formId";
        cmd1.Parameters.AddWithValue("$userId", userId);
        cmd1.Parameters.AddWithValue("formId", formId);
        using var reader1 = cmd1.ExecuteReader();
        while (reader1.Read())
        {
            form.Title = reader1.GetString(0);
            form.Description = reader1.GetString(2);
            form.PrimaryEmotionId = reader1.GetInt32(3);
            form.RecordDate = reader1.GetDateTime(1);
        }

        var cmd2 = connection.CreateCommand();
        cmd2.CommandText = "SELECT emotionId from emotionEntries WHERE moodEntryId = $formId";
        cmd2.Parameters.AddWithValue("$formId", formId);
        using var reader2 = cmd2.ExecuteReader();
        while (reader2.Read())
        {
            form.SelectedEmotionsIds.Add(reader2.GetInt32(0));
        }

        var cmd3 = connection.CreateCommand();
        cmd3.CommandText = "SELECT tagId from tagEntries WHERE moodEntryId = $formId";
        cmd3.Parameters.AddWithValue("$formId", formId);
        using var reader3 = cmd3.ExecuteReader();
        while (reader3.Read())
        {
            form.SelectedTagsIds.Add(reader3.GetInt32(0));
        }

        return form;
    }


    public static List<Emotion> GetAllEmotions(int userId)
    {
        var emotions = new List<Emotion>();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static List<Tag> GetAllTags(int userId)
    {
        var tags = new List<Tag>();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static void AddEmotion(string name, string color, int userId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO emotions (name, color, userId) VALUES ($name, $color, $userId);";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$color", color);
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.ExecuteNonQuery();
    }

    public static void AddTag(string name, int userId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO tags (name, userId) VALUES ($name, $userId);";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.ExecuteNonQuery();
    }

    public static long AddMoodEntry(string title, string description, int primaryEmotionId, int userId, DateTime recordDate)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO moodEntries (name, userId, recordDate, description, primaryEmotionId)
            VALUES ($title, $userId, $date, $desc, $primaryEmotionId);
        ";
        cmd.Parameters.AddWithValue("$title", title);
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$date", recordDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$desc", description ?? "");
        cmd.Parameters.AddWithValue("$primaryEmotionId", primaryEmotionId);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT last_insert_rowid();";
        return (long)cmd.ExecuteScalar();
    }

    public static void AddEmotionToEntry(long moodEntryId, int emotionId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO emotionEntries (moodEntryId, emotionId) VALUES ($meid, $emotionId);";
        cmd.Parameters.AddWithValue("$meid", moodEntryId);
        cmd.Parameters.AddWithValue("$emotionId", emotionId);
        cmd.ExecuteNonQuery();
    }

    public static void AddTagToEntry(long moodEntryId, int tagId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO tagEntries (moodEntryId, tagId) VALUES ($meid, $tagId);";
        cmd.Parameters.AddWithValue("$meid", moodEntryId);
        cmd.Parameters.AddWithValue("$tagId", tagId);
        cmd.ExecuteNonQuery();
    }

    public static void DeleteEmotion(int emotionId, int userId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static void DeleteTag(int tagId, int userId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static List<RecordFormModel> GetAllRecords(int userId)
    {
        List<RecordFormModel> records = new List<RecordFormModel>();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT moodEntryId, name, recordDate, description, primaryEmotionId FROM moodEntries WHERE userId = $userId";
        cmd.Parameters.AddWithValue("$userId", userId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new RecordFormModel
            {
                MoodEntryId = reader.GetInt32(0),
                Title = reader.GetString(1),
                RecordDate = reader.GetDateTime(2),
                Description = reader.GetString(3),
                PrimaryEmotionId = reader.GetInt32(4)
            });
        }

        foreach (var record in records)
        {
            int recordId = record.MoodEntryId;
            using (var emotionCmd = connection.CreateCommand())
            {
                emotionCmd.CommandText = "SELECT emotionId FROM emotionEntries WHERE moodEntryId = $recordId;";
                emotionCmd.Parameters.AddWithValue("$recordId", recordId);
                using var reader1 = emotionCmd.ExecuteReader();
                while (reader1.Read())
                {
                    record.SelectedEmotionsIds.Add(reader1.GetInt32(0));
                }
            }

            using (var tagCmd = connection.CreateCommand())
            {
                tagCmd.CommandText = "SELECT tagId FROM tagEntries WHERE moodEntryId = $recordId;";
                tagCmd.Parameters.AddWithValue("$recordId", recordId);
                using var reader2 = tagCmd.ExecuteReader();
                while (reader2.Read())
                {
                    record.SelectedTagsIds.Add(reader2.GetInt32(0));
                }
            }
        }
        return records;
    }

    public static bool IsUserAdmin(int userId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO users (login, password, token) VALUES ($login, $password, $token);";
        cmd.Parameters.AddWithValue("$login", login);
        cmd.Parameters.AddWithValue("$password", ComputeMD5Hash(password));
        string token = Guid.NewGuid().ToString();
        cmd.Parameters.AddWithValue("$token", token);
        cmd.ExecuteNonQuery();
    }

    public static List<string> GetAllUsers()
    {
        var users = new List<string>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

        var allEmotions = GetAllEmotions(userId);
        var allTags = GetAllTags(userId);

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

            // Находим соответствующие Emotion и Tag
            var emotion = allEmotions.FirstOrDefault(e => e.Name == emotionName);
            var tag = allTags.FirstOrDefault(t => t.Name == tagName);

            // Пропускаем, если что-то не найдено
            if (emotion == null || tag == null)
                continue;

            // Добавляем к результату
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
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static User GetUserByLogin(string login)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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

    public static int? GetUserIdByLogin(string login)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM users WHERE login = $login";
        cmd.Parameters.AddWithValue("$login", login);

        var result = cmd.ExecuteScalar();
        if (result != null)
            return Convert.ToInt32(result);

        return null;
    }

    public static void DeleteRecord(int moodEntryId, int userId)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            DELETE FROM emotionEntries WHERE moodEntryId = $id;
            DELETE FROM tagEntries WHERE moodEntryId = $id;
            DELETE FROM moodEntries WHERE moodEntryId = $id AND userId = $userId;
        ";
        cmd.Parameters.AddWithValue("$id", moodEntryId);
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.ExecuteNonQuery();
    }


    // public static string GetAllMoodEntriesDebug()
    // {
    //     using var connection = new SqliteConnection(connectionString);
    //     connection.Open();

    //     var cmd = connection.CreateCommand();
    //     cmd.CommandText = "SELECT moodEntryId, name, userId, date FROM moodEntries";

    //     using var reader = cmd.ExecuteReader();
    //     var result = new StringBuilder();
    //     result.AppendLine("ВСЕ moodEntries:<br>");

    //     while (reader.Read())
    //     {
    //         int id = reader.GetInt32(0);
    //         string name = reader.GetString(1);
    //         int userId = reader.GetInt32(2);
    //         string date = reader.GetString(3);
    //         result.AppendLine($"ID: {id}, Name: {name}, UserId: {userId}, Date: {date}<br>");
    //     }

    //     return result.ToString();
    // }
}