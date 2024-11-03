namespace Bible.App.Services;

using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Android Logger Provider inspired by
/// <see href="https://stackoverflow.com/a/71715568/16969747"/>
/// </summary>
[SupportedOSPlatform("Android")]
public class AndroidLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            // Remove the namespace from the full class name
            int lastDotPos = categoryName.LastIndexOf('.');
            var category = lastDotPos > 0 ? categoryName[++lastDotPos..] : categoryName;
            return new AndroidLogger(category);
        }
        return NullLogger.Instance;
    }

    public void Dispose() { }
}

[SupportedOSPlatform("Android")]
public class AndroidLogger : ILogger
{
    private readonly string Category;

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public AndroidLogger(string category)
    {
        Category = category;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message = formatter(state, exception);

#if ANDROID
        Java.Lang.Throwable? throwable = null;

        if (exception is not null)
        {
            throwable = Java.Lang.Throwable.FromException(exception);
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
                Android.Util.Log.Verbose(Category, throwable, message);
                break;

            case LogLevel.Debug:
                Android.Util.Log.Debug(Category, throwable, message);
                break;

            case LogLevel.Information:
                Android.Util.Log.Info(Category, throwable, message);
                break;

            case LogLevel.Warning:
                Android.Util.Log.Warn(Category, throwable, message);
                break;

            case LogLevel.Error:
                Android.Util.Log.Error(Category, throwable, message);
                break;

            case LogLevel.Critical:
                Android.Util.Log.Wtf(Category, throwable, message);
                break;
        }
#endif
    }
}