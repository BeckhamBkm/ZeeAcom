namespace ZeeAcom.Common;
using Serilog;
public static class Logging
{
    static Logging()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(@"logs\log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
    }

    public static void LogInfo(string name, object? data = null)
    {
        Log.Information("{Name}: {@Data}", name, data);
    }

    public static void LogException(Exception e)
    {
        Log.Error(e, "Exception occurred");
    }

    public static void Close() => Log.CloseAndFlush();
}


