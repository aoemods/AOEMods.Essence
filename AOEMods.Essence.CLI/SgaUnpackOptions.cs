﻿using CommandLine;

namespace AOEMods.Essence.CLI;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Verb("sga-unpack", HelpText = "Unpacks an SGA archive into a directory.")]
public class SgaUnpackOptions
{
    [Value(0, MetaName = "input-path", Required = true)]
    public string InputPath { get; set; }

    [Value(1, MetaName = "output-path", Required = true)]
    public string OutputPath { get; set; }
    [Option('v', "verbose")]
    public bool Verbose { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.