using Newtonsoft.Json.Linq;
using GeniusAPI;
using GeniusAPI.Models;

namespace last_fm_api;

class Program
{
    static async Task Main(string[] args)
    {
        string accessToken = "y20k-ih6uzRLeiNsq0QLFb8Tl5b8ySYHY_re7E8InIc3oFBjMh-_mavWHPaHv3Gg";
        string apiKey = "e1ee6c709f10d650910e0efe472d70e4"; 
        
        GeniusClient client = new(accessToken);
        
        string track = "Disease";
        string artist = "Beartooth";

        await GetTrackJson(track, artist, apiKey);
        await GetLyrics(client, track, artist);
        await GetArtistTagsFromLastFm(artist, apiKey);
        await GetTrackTagsFromLastFM(apiKey, artist, track);
    }

    static async Task GetLyrics(GeniusClient client, string track, string artist) 
    {
        try
        {
            GeniusTrackInfo? trackInfo = await client.GetTrackInfoAsync(track, artist);

            if (trackInfo != null)
            {
                string lyrics = trackInfo.Lyrics ?? "Текст не найден.";
                Console.WriteLine(lyrics);
            }
            else
            {
                Console.WriteLine("Трек не найден.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
    
    static async Task GetTrackTagsFromLastFM(string apiKey, string artist, string track)
    {
        using HttpClient client = new HttpClient();
        
        // Формируем URL запроса
        string url = $"http://ws.audioscrobbler.com/2.0/?method=track.gettoptags&artist={Uri.EscapeDataString(artist)}&track={Uri.EscapeDataString(track)}&api_key={apiKey}&format=json";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JObject parsedJson = JObject.Parse(json);

            // Извлекаем теги (жанры) трека
            var tags = parsedJson["toptags"]?["tag"];
            if (tags != null && tags.HasValues)
            {
                Console.WriteLine($"Теги (жанры) трека \"{track}\" ({artist}):");
                foreach (var tag in tags)
                {
                    Console.WriteLine($"  - {tag["name"]}");
                }
            }
            else
            {
                Console.WriteLine("Жанры не найдены.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
    
    static async Task GetArtistTagsFromLastFm(string artist, string apiKey)
    {
        using HttpClient client = new HttpClient();

        string url = $"https://ws.audioscrobbler.com/2.0/?method=artist.getTopTags&artist={Uri.EscapeDataString(artist)}&api_key={apiKey}&format=json";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JObject parsedJson = JObject.Parse(json);
            
            JToken? tags = parsedJson["toptags"]?["tag"];
            if (tags != null && tags.HasValues)
            {
                Console.WriteLine($"Теги для артиста {artist}:");
                foreach (var tag in tags)
                {
                    Console.WriteLine($"  - {tag["name"]}");
                }
            }
            else
            {
                Console.WriteLine($"Теги для артиста {artist} не найдены.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
    
    static async Task GetTrackJson(string track, string artist, string apiKey)
    {
        using HttpClient client = new HttpClient();
        
        string url = $"http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key={apiKey}&artist={Uri.EscapeDataString(artist)}&track={Uri.EscapeDataString(track)}&format=json";

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            
            JObject jsonParsed = JObject.Parse(json);
            
            PrintJson(jsonParsed, "");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void PrintJson(JToken token, string indent)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                Console.WriteLine($"{indent}{property.Name}:");
                PrintJson(property.Value, indent + "  ");
            }
        }
        else if (token is JArray array)
        {
            foreach (var item in array)
            {
                PrintJson(item, indent + "  ");
            }
        }
        else
        {
            Console.WriteLine($"{indent}{token}");
        }
    }
}