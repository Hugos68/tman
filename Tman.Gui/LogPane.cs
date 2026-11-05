using System;
using Microsoft.Extensions.Logging;

namespace Tman.Gui
{
    internal sealed class LogPane(Action<string> write) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new PaneLogger(categoryName, write);

        public void Dispose()
        {
        }

        private sealed class PaneLogger(string category, Action<string> write) : ILogger
        {
            private const string Separator = " ";

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }
                var name = category[(category.LastIndexOf('.') + 1)..];
                write(string.Join(Separator, logLevel == LogLevel.Information ? "" : logLevel.ToString().ToUpperInvariant(), name + ":", formatter(state, exception)).Trim());
            }
        }
    }
}
