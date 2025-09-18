using App.Core.Enums;
using HeyRed.Mime;

public class MediaInspector
{
    public static MediaType GetMediaType(Stream stream, string fileName)
    {
        stream.Position = 0;

        var mimeType = MimeGuesser.GuessMimeType(stream);

        stream.Position = 0;

        if (mimeType.StartsWith("image/"))
            return MediaType.Image;

        if (mimeType.StartsWith("video/"))
            return MediaType.Video;

        throw new InvalidOperationException($"Unsupported media type: {mimeType}");
    }

    public static bool IsSafeMedia(Stream stream, string fileName)
    {
        try
        {
            stream.Position = 0;
            var mimeType = MimeGuesser.GuessMimeType(stream);
            stream.Position = 0;

            // Accept only known media types
            return mimeType.StartsWith("image/") || mimeType.StartsWith("video/");
        }
        catch
        {
            return false;
        }
    }
}