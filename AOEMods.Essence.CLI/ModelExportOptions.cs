using CommandLine;

namespace AOEMods.Essence.CLI;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Verb("model-export", HelpText = "Converts a model consisting of a .rrgeom, .rrmaterial and .rrtexture files to .glb.")]
public class ModelExportOptions
{
    [Value(0, MetaName = "input-path", Required = true)]
    public string InputPath { get; set; }
    [Value(1, MetaName = "output-path", Required = true)]
    public string OutputPath { get; set; }
    [Option('v', "verbose")]
    public bool Verbose { get; set; }
    [Option('b', "batch", HelpText = "Treat the input and output path as a directory and convert all .rrgeom files in it.")]
    public bool Batch { get; set; }
    [Option('m', "with-material", HelpText = "Whether to include the material in the conversion.")]
    public bool WithMaterial { get; set; }
    [Option("material-path", HelpText = "Input path for material directory. Will use input's directory if not specified.")]
    public string MaterialInputPath { get; set; }
    [Option("texture-path", HelpText = "Input path for texture directory. Will use material-path if not specified.")]
    public string TextureInputPath { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.