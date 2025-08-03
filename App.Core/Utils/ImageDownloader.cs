namespace App.Core.Utils;

public static class ImageDownloader
{
    public static async Task<Stream> GetImageStreamAsync(string imageUrl)
    {
        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
}