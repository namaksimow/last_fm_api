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
        
        string track = "alone";
        string artist = "i prevail";

        /*var lyrics = await GetLyrics(client, track, artist);*/
        var artistTags = await GetArtistTags(artist, apiKey);
        var trackTags = await GetTrackTags(apiKey, artist, track);
        var tags = await  DefineTrackTags(trackTags, artistTags);

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                Console.WriteLine(tag);
            }    
        }
        else
        {
            Console.WriteLine("No tags found");
        }
        
        
    }

    static async Task<List<JToken>> DefineTrackTags(List<JToken>? trackTags, List<JToken>? artistTags)
    {        
        if (trackTags == null && artistTags == null)
        {
            return null;
        }

        if (trackTags == null)
        {
            return artistTags;
        }
        
        if (artistTags != null)
        {
            List<JToken> tags = artistTags.Intersect(trackTags).ToList();
            return tags;
        }

        return null;
    }
    
    static async Task<string> GetLyrics(GeniusClient client, string track, string artist) 
    {
        GeniusTrackInfo? trackInfo = await client.GetTrackInfoAsync(track, artist);

        if (trackInfo != null)
        {
            string? lyrics = trackInfo.Lyrics;
            return lyrics;
        }
        
        return null;
    }
    
    static async Task<List<JToken>> GetTrackTags(string apiKey, string artist, string track)
    {
        using HttpClient client = new HttpClient();
        
        string url = $"http://ws.audioscrobbler.com/2.0/?method=track.gettoptags&artist={Uri.EscapeDataString(artist)}&track={Uri.EscapeDataString(track)}&api_key={apiKey}&format=json";

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        JObject parsedJson = JObject.Parse(json);
            
        var tags = parsedJson["toptags"]?["tag"];
        if (tags != null && tags.HasValues)
        {
            var popularTags = tags
                .Where(tag => (int?)tag["count"] >= 50)
                .Select(tag => tag["name"])
                .ToList();
                
            return popularTags;   
        }
        
        return null;
    }
    
    static async Task<List<JToken>> GetArtistTags(string artist, string apiKey)
    {
        using HttpClient client = new HttpClient();

        string url = $"https://ws.audioscrobbler.com/2.0/?method=artist.getTopTags&artist={Uri.EscapeDataString(artist)}&api_key={apiKey}&format=json";

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        JObject parsedJson = JObject.Parse(json);
        
        JToken? tags = parsedJson["toptags"]?["tag"];
        if (tags != null && tags.HasValues)
        {
            var popularTags = tags
                .Where(tag => (int?)tag["count"] >= 50)
                .Select(tag => tag["name"])
                .ToList();

            return popularTags;
        }
        
        return null;
    }
}