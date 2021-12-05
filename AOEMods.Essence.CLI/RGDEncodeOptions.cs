using CommandLine;

namespace AOEMods.Essence.CLI;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Verb("rgd-encode", HelpText = "Converts an xml file to an RGD file.")]
public class RGDEncodeOptions
{
    [Value(0, MetaName = "input-path", Required = true)]
    public string InputPath { get; set; }
    [Value(1, MetaName = "output-path", Required = true)]
    public string OutputPath { get; set; }
    [Option('v', "verbose")]
    public bool Verbose { get; set; }
    [Option('b', "batch", HelpText = "Treat the input and output path as a directory and convert all .json files in it.")]
    public bool Batch { get; set; }

    [Option('f', "format", HelpText = "The input format to convert from. Only xml is supported.", Default = "xml")]
    public string Format { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.