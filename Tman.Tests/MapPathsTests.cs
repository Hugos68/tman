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
            Assert.Equal(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Out.Map.Gbx")), paths.OutputFor(TestServices.TestMap));
        }

        [Fact]
        public void AWildcardInputNeedsAWildcardOutputAndNamesEachFileAfterItsInput()
        {
            var folder = Path.GetDirectoryName(TestServices.TestMap)!;
            var pattern = Path.Combine(folder, "*.Map.Gbx");
            Assert.Throws<ArgumentException>(() => MapPaths.Resolve(pattern, Path.Combine(folder, "Out.Map.Gbx")));
            Assert.Throws<ArgumentException>(() => MapPaths.Resolve(TestServices.TestMap, Path.Combine(folder, "* Wood.Map.Gbx")));
            var paths = MapPaths.Resolve(pattern, Path.Combine(folder, "Altered", "* Wood.Map.Gbx"));
            Assert.Contains(TestServices.TestMap, paths.Inputs);
            Assert.Equal(Path.GetFullPath(Path.Combine(folder, "Altered", "Test Wood.Map.Gbx")), paths.OutputFor(TestServices.TestMap));
        }

        [Fact]
        public void MissingInputsAreRefused()
        {
            Assert.Throws<FileNotFoundException>(() => MapPaths.Resolve(Path.Combine(Path.GetDirectoryName(TestServices.TestMap)!, "Nope.Map.Gbx"), "x.Map.Gbx"));
            Assert.Throws<DirectoryNotFoundException>(() => MapPaths.Resolve(Path.Combine(Path.GetTempPath(), "no-such-folder-tman", "*.Map.Gbx"), "*.Map.Gbx"));
        }

        [Fact]
        public void AFolderTargetNamesEveryOutputAfterItsInput()
        {
            var paths = MapPaths.ForFolder([TestServices.TestMap], Path.Combine(Path.GetTempPath(), "Altered"));
            Assert.Equal(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Altered", "Test.Map.Gbx")), paths.OutputFor(TestServices.TestMap));
        }
    }
}
