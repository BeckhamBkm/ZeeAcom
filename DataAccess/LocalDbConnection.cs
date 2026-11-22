using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ZeeAcom.DataAccess;

/// <summary>
/// Development-only workaround for SQL Server LocalDB.
/// <para>
/// A "(localdb)\Instance" connection makes SqlClient go through the LocalDB API, which on some machines
/// fails under the Visual Studio debugger ("Error occurred during LocalDB instance startup: SQL Server
/// process failed to start") and can leave the instance in a stuck state. Instead, each time a connection
/// is opened this points it at a running LocalDB named pipe that serves this app's database (starting the
/// instance with the SqlLocalDB tool if none is found). The pipe is cached and looked up again after a
/// connection failure, so restarting LocalDB doesn't need an app restart. It never falls back to the
/// LocalDB API: if the instance can't be reached you get an error saying how to start it.
/// </para>
/// </summary>
public sealed partial class LocalDbConnection(string instance, ILogger logger) : DbConnectionInterceptor
{
    private volatile string? pipe;

    /// <summary>The LocalDB instance name if the connection string uses "(localdb)\Instance", otherwise null.</summary>
    public static string? GetInstanceName(string connectionString)
    {
        var match = LocalDbDataSource().Match(new SqlConnectionStringBuilder(connectionString).DataSource);
        return match.Success ? match.Groups["instance"].Value : null;
    }

    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        PointAtPipe(connection);
        return result;
    }

    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = default)
    {
        PointAtPipe(connection);
        return ValueTask.FromResult(result);
    }

    // The instance may have been restarted (new pipe name) or stopped: look it up again next time.
    public override void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData) => pipe = null;

    public override Task ConnectionFailedAsync(DbConnection connection, ConnectionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        pipe = null;
        return Task.CompletedTask;
    }

    private void PointAtPipe(DbConnection connection)
    {
        var current = pipe ??= FindOrStartPipe(connection.ConnectionString)
            ?? throw new InvalidOperationException(
                $"LocalDB instance '{instance}' isn't running and couldn't be started. In a terminal run " +
                $"`SqlLocalDB start {instance}` (if that fails, run `SqlLocalDB stop {instance} -k` first), then reload the page.");

        var builder = new SqlConnectionStringBuilder(connection.ConnectionString) { DataSource = current };
        if (builder.ConnectionString != connection.ConnectionString)
        {
            connection.ConnectionString = builder.ConnectionString;
        }
    }

    private string? FindOrStartPipe(string connectionString)
    {
        try
        {
            // `SqlLocalDB info` isn't reliable here: processes started from Visual Studio and from a normal
            // terminal can each report the other's running instance as "Stopped". The pipes themselves are
            // visible to everyone, so look for a LocalDB pipe that actually serves this app's database.
            var found = FindWorkingPipe(connectionString);
            if (found is null)
            {
                var info = RunSqlLocalDb(DefaultTool, $"info \"{instance}\"");
                var tool = ToolForVersion(info) ?? DefaultTool;
                logger.LogInformation("Starting LocalDB instance {Instance} with {Tool}", instance, tool);
                RunSqlLocalDb(tool, $"start \"{instance}\"");

                for (var attempt = 0; found is null && attempt < 5; attempt++)
                {
                    found = ParsePipe(RunSqlLocalDb(DefaultTool, $"info \"{instance}\"")) ?? FindWorkingPipe(connectionString);
                    if (found is null)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            if (found is null)
            {
                logger.LogWarning("LocalDB instance {Instance} isn't running and couldn't be started.", instance);
            }
            else
            {
                logger.LogInformation("Using LocalDB instance {Instance} via named pipe {Pipe}", instance, found);
            }

            return found;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't look up LocalDB instance {Instance}.", instance);
            return null;
        }
    }

    /// <summary>
    /// The first running LocalDB pipe (\\.\pipe\LOCALDB#xxxx\tsql\query) that accepts a connection to
    /// this app's database. Other LocalDB instances on the machine won't have it, so they're skipped.
    /// </summary>
    private static string? FindWorkingPipe(string connectionString)
    {
        foreach (var path in Directory.GetFiles(@"\\.\pipe\").Where(p => LocalDbPipePath().IsMatch(p)))
        {
            var candidate = "np:" + path;
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                DataSource = candidate,
                ConnectTimeout = 3,
                Pooling = false,
            };

            try
            {
                using var probe = new SqlConnection(builder.ConnectionString);
                probe.Open();
                return candidate;
            }
            catch (SqlException)
            {
                // Not this app's instance (or not ready yet); try the next one.
            }
        }

        return null;
    }

    private const string DefaultTool = "SqlLocalDB.exe";

    private static string? ToolForVersion(string info)
    {
        var version = VersionLine().Match(info);
        if (!version.Success)
        {
            return null;
        }

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Microsoft SQL Server", $"{version.Groups["major"].Value}0", "Tools", "Binn", "SqlLocalDB.exe");
        return File.Exists(path) ? path : null;
    }

    private static string? ParsePipe(string info)
    {
        var match = PipeLine().Match(info);
        return match.Success ? match.Groups["pipe"].Value.Trim() : null;
    }

    private static string RunSqlLocalDb(string tool, string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo(tool, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        })!;

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(30_000);
        return output;
    }

    // "(localdb)\MSSQLLocalDB" -> MSSQLLocalDB
    [GeneratedRegex(@"^\(localdb\)\\(?<instance>[^\\;]+)$", RegexOptions.IgnoreCase)]
    private static partial Regex LocalDbDataSource();

    // "Instance pipe name: np:\\.\pipe\LOCALDB#1234ABCD\tsql\query" (empty while stopped)
    [GeneratedRegex(@"pipe name:\s*(?<pipe>np:\S+)", RegexOptions.IgnoreCase)]
    private static partial Regex PipeLine();

    // \\.\pipe\LOCALDB#1234ABCD\tsql\query
    [GeneratedRegex(@"^\\\\\.\\pipe\\LOCALDB#[0-9A-F]+\\tsql\\query$", RegexOptions.IgnoreCase)]
    private static partial Regex LocalDbPipePath();

    // "Version:            15.0.4382.1"
    [GeneratedRegex(@"Version:\s*(?<major>\d+)\.", RegexOptions.IgnoreCase)]
    private static partial Regex VersionLine();
}
