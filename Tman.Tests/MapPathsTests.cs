using System;
using System.IO;
using Tman.Interface;
using Xunit;

namespace Tman.Tests
{
    public class MapPathsTests
    {
        [Fact]
        public void ASingleInputWritesToTheOutputAsGiven()
        {
            var paths = MapPaths.Resolve(TestServices.TestMap, Path.Combine(Path.GetTempPath(), "Out.Map.Gbx"));
            Assert.Equal([TestServices.TestMap], paths.Inputs);
            Assert.Equal(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Out.Map.Gbx")), paths.OutputFor("test Wood"));
        }

        [Fact]
        public void AWildcardOutputIsNamedAfterTheAlteredMap()
        {
            var folder = Path.GetDirectoryName(TestServices.TestMap)!;
            var pattern = Path.Combine(folder, "*.Map.Gbx");
            Assert.Throws<ArgumentException>(() => MapPaths.Resolve(pattern, Path.Combine(folder, "Out.Map.Gbx")));
            Assert.Throws<ArgumentException>(() => MapPaths.Resolve(TestServices.TestMap, Path.Combine(folder, "*.Map.Gbx")));
            var paths = MapPaths.Resolve(pattern, Path.Combine(folder, "Altered", "*.Map.Gbx"));
            Assert.Contains(TestServices.TestMap, paths.Inputs);
            Assert.Equal(Path.GetFullPath(Path.Combine(folder, "Altered", "test Wood.Map.Gbx")), paths.OutputFor("test Wood"));
        }

        [Fact]
        public void MissingInputsAreRefused()
        {
            Assert.Throws<FileNotFoundException>(() => MapPaths.Resolve(Path.Combine(Path.GetDirectoryName(TestServices.TestMap)!, "Nope.Map.Gbx"), "x.Map.Gbx"));
            Assert.Throws<DirectoryNotFoundException>(() => MapPaths.Resolve(Path.Combine(Path.GetTempPath(), "no-such-folder-tman", "*.Map.Gbx"), "*.Map.Gbx"));
        }

        [Fact]
        public void AFolderTargetNamesEveryOutputAfterItsMap()
        {
            var paths = MapPaths.ForFolder([TestServices.TestMap], Path.Combine(Path.GetTempPath(), "Altered"));
            Assert.Equal(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Altered", "Fall 2026 - 05 Wood.Map.Gbx")), paths.OutputFor("Fall 2026 - 05 Wood"));
        }

        [Theory]
        [InlineData("$f00Fall $o2026$z - 01 Wood", "Fall 2026 - 01 Wood")]
        [InlineData("$$100 $s$iTrack", "$100 Track")]
        [InlineData("$l[https://example.com]Linked$l map", "Linked map")]
        [InlineData("A/B:C*D?E\"F<G>H|I", "A_B_C_D_E_F_G_H_I")]
        [InlineData("  trailing. ", "trailing")]
        [InlineData("$fff", "_")]
        public void TheFileNameIsTheDeformattedMapNameWithoutInvalidCharacters(string mapName, string fileName)
        {
            Assert.Equal(fileName, MapNames.FileName(mapName));
        }
    }
}
