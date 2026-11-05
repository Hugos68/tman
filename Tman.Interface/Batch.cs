using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tman.Core.Alterations;
using Tman.Core.Maps;

namespace Tman.Interface
{
    public static class Batch
    {
        public static async Task RunAsync(AlterationPipeline pipeline, string alterationName, MapPaths paths, Action<int, int, string> progress, CancellationToken cancellationToken)
        {
            var done = 0;
            await Parallel.ForEachAsync(paths.Inputs, cancellationToken, (input, token) =>
            {
                token.ThrowIfCancellationRequested();
                var output = Apply(pipeline, alterationName, input, paths.OutputFor(input));
                progress(Interlocked.Increment(ref done), paths.Inputs.Count, output);
                return ValueTask.CompletedTask;
            });
        }

        public static string Apply(AlterationPipeline pipeline, string alterationName, string input, string output)
        {
            var map = Map.Load(input);
            pipeline.Apply(alterationName, map);
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            map.Save(output);
            return output;
        }
    }
}
