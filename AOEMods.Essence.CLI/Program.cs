using AOEMods.Essence.CLI;
using CommandLine;

int OnError(IEnumerable<Error> errors)
{
    return 1;
}

return Parser.Default.ParseArguments<SgaPackOptions, SgaUnpackOptions, RRTexDecodeOptions, RGDEncodeOptions, RGDDecodeOptions, RRGeomDecodeOptions, ModelExportOptions>(args)
    .MapResult<SgaPackOptions, SgaUnpackOptions, RRTexDecodeOptions, RGDEncodeOptions, RGDDecodeOptions, RRGeomDecodeOptions, ModelExportOptions, int>(
        Commands.SgaPack,
        Commands.SgaUnpack,
        Commands.RRTexDecode,
        Commands.RGDEncode,
        Commands.RGDDecode,
        Commands.RRGeomDecode,
        Commands.ModelExport,
        OnError
    );
