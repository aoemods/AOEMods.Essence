using System;
using System.IO;
using Xunit;

namespace AOEMods.Essence.CLI.Test
{
    public class TestCommands
    {
        [Fact]
        public void SgaPackSgaUnpack_DirectoryWithTextFile_SameText()
        {
            // Write a test folder structure:
            // - root
            //   - testfile.txt: "hello world"
            string inPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(inPath);

            string textFileText = "hello world";
            string textFileName = "testfile.txt";
            File.WriteAllText(Path.Combine(inPath, textFileName), textFileText);

            string outPath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".sga"));
            string archiveName = "test";

            // Pack the test folder into an sga archive
            var options = new SgaPackOptions()
            {
                InputPath = inPath,
                OutputPath = outPath,
                ArchiveName = archiveName,
            };

            int resultCode = Commands.SgaPack(options);

            Assert.Equal(0, resultCode);
            Assert.True(File.Exists(outPath));

            string extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Read sga file we just created and recreate the folder structure
            resultCode = Commands.SgaUnpack(new SgaUnpackOptions()
            {
                InputPath = outPath,
                OutputPath = extractPath,
            });

            Assert.Equal(0, resultCode);
            string restoredText = File.ReadAllText(Path.Combine(extractPath, textFileName));
            Assert.Equal(textFileText, restoredText);
        }
    }
}