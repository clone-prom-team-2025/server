using SkiaSharp;

namespace App.Core.Utils;

public class AvatarGenerator
{
    public static byte[] CreateAvatar(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty");

        const int size = 512;
        char firstLetter = char.ToUpper(name.Trim()[0]);

        using var surface = SKSurface.Create(new SKImageInfo(size, size));
        var canvas = surface.Canvas;

        var backgroundColor = GetReadableRandomBackground();
        canvas.Clear(backgroundColor);

        using var font = new SKFont();
        font.Size = size / 2f;
        font.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);

        using var paint = new SKPaint();
        paint.Color = SKColors.White;
        paint.IsAntialias = true;

        var text = firstLetter.ToString();
        var x = size / 2f;
        var y = size / 2f - (font.Metrics.Ascent + font.Metrics.Descent) / 2;

        canvas.DrawText(text, x, y, SKTextAlign.Center, font, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    private static SKColor GetReadableRandomBackground()
    {
        var rnd = new Random();
        while (true)
        {
            byte r = (byte)rnd.Next(256);
            byte g = (byte)rnd.Next(256);
            byte b = (byte)rnd.Next(256);

            var luminance = 0.299 * r + 0.587 * g + 0.114 * b;
            if (luminance < 128)
                return new SKColor(r, g, b);
        }
    }
    
    public static Stream ByteToStream(byte[] data)
    {
        return new MemoryStream(data);
    }
}