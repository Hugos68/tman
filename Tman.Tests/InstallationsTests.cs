using Tman.Paks.Installations;
using Xunit;

namespace Tman.Tests
{
    public class InstallationsTests
    {
        [Fact]
        public void SteamLibraryFoldersListEveryLibraryPath()
        {
            const string LibraryFolders = """
                "libraryfolders"
                {
                	"0"
                	{
                		"path"		"C:\\Program Files (x86)\\Steam"
                		"label"		""
                	}
                	"1"
                	{
                		"path"		"D:\\SteamLibrary"
                	}
                }
                """;
            Assert.Equal([@"C:\Program Files (x86)\Steam", @"D:\SteamLibrary"], SteamLibraries.Parse(LibraryFolders));
        }

        [Fact]
        public void EpicManifestsNameTheInstallLocation()
        {
            Assert.Equal(@"D:\Games\Trackmania", EpicInstalls.InstallLocation("""{"FormatVersion": 0, "DisplayName": "Trackmania", "InstallLocation": "D:\\Games\\Trackmania"}"""));
            Assert.Null(EpicInstalls.InstallLocation("""{"FormatVersion": 0}"""));
        }
    }
}
