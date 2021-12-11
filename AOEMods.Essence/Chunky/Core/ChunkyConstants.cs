namespace AOEMods.Essence.Chunky.Core;

/// <summary>
/// Constants related to the Relic Chunky format.
/// </summary>
public static class ChunkyConstants
{
    /// <summary>
    /// Magic bytes "Relic Chunky\x0D\x0A\x1A\0" of the Relic Chunky format.
    /// </summary>
    public static byte[] Magic { get; } = (new[] { 'R', 'e', 'l', 'i', 'c', ' ', 'C', 'h', 'u', 'n', 'k', 'y', '\x0D', '\x0A', '\x1A', '\0' }).Select(c => (byte)c).ToArray();
}
