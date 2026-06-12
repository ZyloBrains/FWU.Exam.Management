using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Web.Logging;

public class FileLogger : ILogger
{
    private readonly string _infoPath;
    private readonly string _errorPath;
    private readonly string _category;
    private readonly object _lock = new();

    public FileLogger(string infoPath, string errorPath, string category)
    {
        _infoPath = infoPath;
        _errorPath = errorPath;
        _category = category;
        var dir = Path.GetDirectoryName(_infoPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logLine = $"[{timestamp}] [{logLevel}] [{_category}] {message}";
        if (exception != null)
            logLine += $"\n{exception}";

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_infoPath, logLine + "\n");

                if (logLevel >= LogLevel.Error)
                    File.AppendAllText(_errorPath, logLine + "\n");
            }
            catch
            {
                // Silently ignore file write errors
            }
        }
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _infoPath;
    private readonly string _errorPath;

    public FileLoggerProvider(string logDirectory)
    {
        _infoPath = Path.Combine(logDirectory, "info.log");
        _errorPath = Path.Combine(logDirectory, "error.log");
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(_infoPath, _errorPath, categoryName);

    public void Dispose() { }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string logDirectory)
    {
        builder.AddProvider(new FileLoggerProvider(logDirectory));
        return builder;
    }
}
