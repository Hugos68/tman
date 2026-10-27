using System.CommandLine;

namespace tman.Generator;

public static class Program
{
    public static int Main(string[] args)
    {
        var paks = new Option<DirectoryInfo>("--paks") { Required = true };
        var output = new Option<DirectoryInfo>("--out") { Required = true };
        var project = new Option<DirectoryInfo?>("--project");
        var custom = new Option<string?>("--custom");
        var root = new RootCommand { paks, output, project, custom };
        root.SetAction(result =>
        {
            Generator.Run(result.GetValue(paks)!.FullName, result.GetValue(output)!.FullName, result.GetValue(project)?.FullName, result.GetValue(custom));
        });
        return root.Parse(args).Invoke();
    }
}
