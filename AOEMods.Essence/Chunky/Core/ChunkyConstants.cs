namespace AOEMods.Essence.Chunky.Core;

public static class ChunkyConstants
{
    public static byte[] Magic { get; } = (new[] { 'R', 'e', 'l', 'i', 'c', ' ', 'C', 'h', 'u', 'n', 'k', 'y', '\x0D', '\x0A', '\x1A', '\0' }).Select(c => (byte)c).ToArray();
}
