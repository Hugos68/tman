using System;
using System.Collections.Generic;
using GBX.NET;
using Tman.Paks;

namespace Tman.Tests.Fakes
{
    internal sealed class NoPaks : IPakStore
    {
        public string Directory => throw Touched();

        public IEnumerable<string> Entries => throw Touched();

        public string? Resolve(string logical) => throw Touched();

        public Gbx? Open(string logical) => throw Touched();

        public byte[]? ReadDecompressed(string logical) => throw Touched();

        private static InvalidOperationException Touched() => new("The test reached for the game's paks");
    }
}
