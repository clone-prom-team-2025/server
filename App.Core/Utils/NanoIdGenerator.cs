using NanoidDotNet;

namespace MyApp.Core.Utils;

/// <summary>
///     Provides a utility method for generating unique NanoId strings
///     with a predefined alphanumeric alphabet.
/// </summary>
public static class NanoIdGenerator
{
    /// <summary>
    ///     The allowed characters used in generated IDs.
    ///     Includes lowercase, uppercase letters, and digits.
    /// </summary>
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    ///     Generates a NanoId string with the specified length using the default alphanumeric alphabet.
    /// </summary>
    /// <param name="length">The desired length of the generated ID.</param>
    /// <returns>A random alphanumeric string of the specified length.</returns>
    public static string Generate(int length)
    {
        return Nanoid.Generate(Alphabet, length);
    }
}