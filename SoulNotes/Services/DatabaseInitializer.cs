using SoulNotes.Models;

namespace SoulNotes.Services
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();

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
                string hashedPassword = Db.ComputeMD5Hash("Mercy");
                cmd.CommandText = "INSERT INTO users (login, password, token) VALUES ('Playlist', $password, 'playlist');";
                cmd.Parameters.AddWithValue("$password", hashedPassword);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                cmd.CommandText = "INSERT INTO users (login, password, isAdmin, token) VALUES ('admin', $password, 1, 'admin');";
                cmd.Parameters.AddWithValue("$password", Db.ComputeMD5Hash("admin"));
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
    }
}