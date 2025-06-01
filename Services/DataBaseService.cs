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
            password TEXT NOT NULL
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
            date TEXT NOT NULL,
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
            cmd.CommandText = "INSERT INTO users (login, password) VALUES ('Playlist', $password);";
            cmd.Parameters.AddWithValue("$password", hashedPassword);
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

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name, date, description, primaryEmotionId FROM moodEntries WHERE userId = $userId AND moodEntryId = $formId";
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("formId", formId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            form.Title = reader.GetString(0);
            form.Description = reader.GetString(2);
            form.PrimaryEmotionId = reader.GetInt32(3);
            form.RecordDate = reader.GetDateTime(1);
        }

        cmd.Parameters.Clear();
        cmd.CommandText = "SELECT emotionId from emotionEntries WHERE moodEntryId = $formId";
        cmd.Parameters.AddWithValue("$formId", formId);
        using var reader2 = cmd.ExecuteReader();
        while (reader2.Read())
        {
            form.SelectedEmotionsIds.Add(reader2.GetInt32(0));
        }

        cmd.Parameters.Clear();
        cmd.CommandText = "SELECT tagId from emotionEntries WHERE moodEntryId = $formId";
        cmd.Parameters.AddWithValue("$formId", formId);
        using var reader3 = cmd.ExecuteReader();
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
        Console.WriteLine("Adding mood entry: " + title + " / " + userId + " / " + recordDate);
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO moodEntries (name, userId, date, description, primaryEmotionId)
            VALUES ($title, $userId, $date, $desc, $primaryEmotionId);
        ";
        cmd.Parameters.AddWithValue("$title", title);
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$date", recordDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$desc", description);
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
        cmd.CommandText = "SELECT moodEntryId, name, date, description, primaryEmotionId FROM moodEntries WHERE userId = $userId";
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