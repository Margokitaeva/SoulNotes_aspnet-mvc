using SoulNotes.Models;

namespace SoulNotes.Services
{
    public static class RecordService
    {
        public static RecordFormModel GetFormData(int formId, int userId)
        {
            if (formId == -1)
                return null;
            RecordFormModel form = new RecordFormModel();
            using var connection = Db.OpenConnection();

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

        public static long AddMoodEntry(string title, string description, int primaryEmotionId, int userId, DateTime recordDate)
        {
            using var connection = Db.OpenConnection();

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
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO emotionEntries (moodEntryId, emotionId) VALUES ($meid, $emotionId);";
            cmd.Parameters.AddWithValue("$meid", moodEntryId);
            cmd.Parameters.AddWithValue("$emotionId", emotionId);
            cmd.ExecuteNonQuery();
        }

        public static void AddTagToEntry(long moodEntryId, int tagId)
        {
            using var connection = Db.OpenConnection();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO tagEntries (moodEntryId, tagId) VALUES ($meid, $tagId);";
            cmd.Parameters.AddWithValue("$meid", moodEntryId);
            cmd.Parameters.AddWithValue("$tagId", tagId);
            cmd.ExecuteNonQuery();
        }

        public static List<RecordFormModel> GetAllRecords(int userId)
        {
            List<RecordFormModel> records = new List<RecordFormModel>();
            using var connection = Db.OpenConnection();

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

        public static void DeleteRecord(int moodEntryId, int userId)
        {
            using var connection = Db.OpenConnection();

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



    }

}

