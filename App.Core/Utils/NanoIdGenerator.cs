using NanoidDotNet;

namespace MyApp.Core.Utils;

public static class NanoIdGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length)
    {
        return Nanoid.Generate(Alphabet, length);
    }
}