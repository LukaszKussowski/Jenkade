using Avalonia.Logging;

namespace Jenkade.App.Tests;

/// <summary>Collects binding warnings and errors that Avalonia logs while it is installed.</summary>
public sealed class BindingErrorLog : ILogSink, IDisposable
{
    private readonly ILogSink? previous;
    private readonly List<string> messages = [];

    private BindingErrorLog()
    {
        previous = Logger.Sink;
        Logger.Sink = this;
    }

    public IReadOnlyList<string> Messages => messages;

    public static BindingErrorLog Capture() => new();

    public bool IsEnabled(LogEventLevel level, string area) =>
        area == LogArea.Binding && level >= LogEventLevel.Warning;

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate) =>
        Log(level, area, source, messageTemplate, []);

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        if (IsEnabled(level, area))
            messages.Add($"{messageTemplate} [{string.Join(", ", propertyValues)}] (source: {source})");
    }

    public void Dispose() => Logger.Sink = previous;
}
