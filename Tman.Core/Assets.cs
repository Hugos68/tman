namespace Tman.Core
{
    internal static class Assets
    {
        private const string TemplatesFolder = "Assets/Templates/";

        public static string Template(string file) => TemplatesFolder + file;
    }
}
