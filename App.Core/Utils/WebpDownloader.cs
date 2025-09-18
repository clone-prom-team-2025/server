namespace App.Core.Utils;

public class WebpDownloader
{
    private static readonly HttpClient HttpClient = new HttpClient();
    
    public static async Task<Stream> GetWebpStreamAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }  
}