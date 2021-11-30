using CommandLine;

namespace AOEMods.Essence.CLI;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Verb("rrtex-decode", HelpText = "Converts an RRTex texture file into an image.")]
public class RRTexDecodeOptions
{
    [Value(0, MetaName = "input-path", Required = true)]
    public string InputPath { get; set; }
    [Value(1, MetaName = "output-path", Required = true)]
    public string OutputPath { get; set; }
    [Option('v', "verbose")]
    public bool Verbose { get; set; }
    [Option('a', "all-mips", HelpText = "Write all mips instead of just the highest one. Output file name will have the mip index prepended.")]
    public bool AllMips { get; set; }
    [Option('b', "batch", HelpText = "Treat the input and output path as a directory and convert all .rrtex files in it.")]
    public bool Batch { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.