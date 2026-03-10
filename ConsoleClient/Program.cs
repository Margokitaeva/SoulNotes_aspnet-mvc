using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    static readonly string baseUrl = "http://localhost:5133/api"; 
    static readonly string login = "Playlist";
    static readonly string token = "playlist";

    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("\n=== Menu ===");
            Console.WriteLine("1. Show emotions");
            Console.WriteLine("2. Add emotion");
            Console.WriteLine("3. Delete emotion");
            Console.WriteLine("4. Show tags");
            Console.WriteLine("5. Add tag");
            Console.WriteLine("6. Delete tag");
            // Console.WriteLine("7. Show records");
            // Console.WriteLine("8. Add record");
            // Console.WriteLine("9. Delete record");
            Console.WriteLine("0. Exit");

            Console.Write("Choose: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await ShowEmotions(); break;
                case "2": await AddEmotion(); break;
                case "3": await DeleteEmotion(); break;
                case "4": await ShowTags(); break;
                case "5": await AddTag(); break;
                case "6": await DeleteTag(); break;
                // case "7": await ShowMoodEntries(); break;
                // case "8": await AddMoodEntry(); break;
                // case "9": await DeleteMoodEntry(); break;
                case "0": return;
                default: Console.WriteLine("Wrong input"); break;
            }
        }
    }

    static async Task<int> GetEmotionIdByName(string name)
    {
        var url = $"{baseUrl}/emotions?login={login}&token={token}";
        var client = new HttpClient();
        var response = await client.GetFromJsonAsync<List<Emotion>>(url);
        var emotion = response?.Find(e => e.Name.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        return emotion?.EmotionId ?? -1;
    }

    static async Task<int> GetTagIdByName(string name)
    {
        var url = $"{baseUrl}/tags?login={login}&token={token}";
        var client = new HttpClient();
        var tags = await client.GetFromJsonAsync<List<Tag>>(url);
        var tag = tags?.Find(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return tag?.TagId ?? -1;
    }

    class Emotion
    {
        public int EmotionId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    class Tag
    {
        public int TagId { get; set; }
        public string Name { get; set; }
    }

    class MoodEntry
    {
        public int MoodEntryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int PrimaryEmotionId { get; set; }
    }

    static async Task ShowEmotions()
    {
        var url = $"{baseUrl}/emotions?login={login}&token={token}";
        var client = new HttpClient();
        var emotions = await client.GetFromJsonAsync<List<Emotion>>(url);

        if (emotions == null || emotions.Count == 0)
        {
            Console.WriteLine("Emotions not found.");
            return;
        }

        Console.WriteLine("\nList of emotions:");
        foreach (var e in emotions)
        {
            Console.WriteLine($"Name: {e.Name}, Color: {e.Color}");
        }
    }

    static async Task<int> GetMoodEntryIdByTitle(string title)
    {
        var url = $"{baseUrl}/moodentries?login={login}&token={token}";
        var client = new HttpClient();
        var entries = await client.GetFromJsonAsync<List<MoodEntry>>(url);
        var entry = entries?.Find(e => e.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        return entry?.MoodEntryId ?? -1;
    }

    static async Task AddEmotion()
    {
        Console.Write("Emotion name: ");
        var name = Console.ReadLine();
        Console.Write("Color (hex): ");
        var color = Console.ReadLine();

        var url = $"{baseUrl}/emotions?login={login}&token={token}";
        var client = new HttpClient();
        var body = new { name = name, color = color };
        var response = await client.PostAsJsonAsync(url, body);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    static async Task DeleteEmotion()
    {
        var url = $"{baseUrl}/emotions?login={login}&token={token}";
        var client = new HttpClient();
        var emotions = await client.GetFromJsonAsync<List<Emotion>>(url);

        if (emotions == null || emotions.Count == 0)
        {
            Console.WriteLine("No emotions found.");
            return;
        }

        Console.WriteLine("\nList of emotions:");
        foreach (var e in emotions)
        {
            Console.WriteLine($"ID: {e.EmotionId}, Name: {e.Name}, Color: {e.Color}");
        }

        Console.Write("Enter ID of emotion to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var deleteUrl = $"{baseUrl}/emotions/{id}?login={login}&token={token}";
        var response = await client.DeleteAsync(deleteUrl);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    static async Task ShowTags()
    {
        var url = $"{baseUrl}/tags?login={login}&token={token}";
        var client = new HttpClient();
        var tags = await client.GetFromJsonAsync<List<Tag>>(url);

        if (tags == null || tags.Count == 0)
        {
            Console.WriteLine("Tags not found.");
            return;
        }

        Console.WriteLine("\nList of tags:");
        foreach (var t in tags)
        {
            Console.WriteLine($"Name: {t.Name}");
        }
    }

    static async Task AddTag()
    {
        Console.Write("Tag name: ");
        var name = Console.ReadLine();

        var url = $"{baseUrl}/tags?login={login}&token={token}";
        var client = new HttpClient();
        var body = new { name = name };
        var response = await client.PostAsJsonAsync(url, body);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    static async Task DeleteTag()
    {
        var url = $"{baseUrl}/tags?login={login}&token={token}";
        var client = new HttpClient();
        var tags = await client.GetFromJsonAsync<List<Tag>>(url);

        if (tags == null || tags.Count == 0)
        {
            Console.WriteLine("No tags found.");
            return;
        }

        Console.WriteLine("\nList of tags:");
        foreach (var t in tags)
        {
            Console.WriteLine($"ID: {t.TagId}, Name: {t.Name}");
        }

        Console.Write("Enter ID of tag to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        var deleteUrl = $"{baseUrl}/tags/{id}?login={login}&token={token}";
        var response = await client.DeleteAsync(deleteUrl);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }


    static async Task ShowMoodEntries()
    {
        var url = $"{baseUrl}/moodentries?login={login}&token={token}";
        var client = new HttpClient();
        var entries = await client.GetFromJsonAsync<List<MoodEntry>>(url);

        if (entries == null || entries.Count == 0)
        {
            Console.WriteLine("Records not found.");
            return;
        }

        Console.WriteLine("\nList of records:");
        foreach (var entry in entries)
        {
            Console.WriteLine($"Name: {entry.Title}, \nDecription: {entry.Description}");
        }
    }

    static async Task AddMoodEntry()
    {
        Console.Write("Title: ");
        var title = Console.ReadLine();
        Console.Write("Description: ");
        var description = Console.ReadLine();
        Console.Write("Primary emotion: ");
        var emotionName = Console.ReadLine();
        int primaryEmotionId = await GetEmotionIdByName(emotionName);
        if (primaryEmotionId == -1)
        {
            Console.WriteLine("Emotion not found.");
            return;
        }

        var url = $"{baseUrl}/moodentries?login={login}&token={token}";
        var client = new HttpClient();
        var body = new
        {
            title = title,
            description = description,
            primaryEmotionId = primaryEmotionId
            
        };
        var response = await client.PostAsJsonAsync(url, body);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    static async Task DeleteMoodEntry()
    {
        Console.Write("Record name: ");
        var title = Console.ReadLine();
        int id = await GetMoodEntryIdByTitle(title);
        if (id == -1)
        {
            Console.WriteLine("Record not found.");
            return;
        }

        var url = $"{baseUrl}/moodentries/{id}?login={login}&token={token}";
        var client = new HttpClient();
        var response = await client.DeleteAsync(url);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

}

