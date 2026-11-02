namespace Tman.Core
{
    internal static class Assets
    {
        private const string DataFolder = "Assets/Data/";

        public static string Data(string file) => DataFolder + file;
    }
}
