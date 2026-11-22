using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ZeeAcom.DataAccess;

/// <summary>
/// Development-only helper for SQL Server LocalDB.
/// <para>
/// A "(localdb)\Instance" connection makes SqlClient call the LocalDB API on every new connection,
/// and on some machines that API fails to start SQL Server when the app runs under the Visual Studio
/// debugger ("Error occurred during LocalDB instance startup: SQL Server process failed to start").
/// This starts the instance with the SqlLocalDB tool and connects straight to its named pipe instead,
/// which skips the LocalDB API entirely. Non-LocalDB connection strings are returned unchanged.
/// </para>
/// </summary>
public static partial class LocalDbConnection
{
    public static string Resolve(string connectionString, ILogger logger)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var match = LocalDbDataSource().Match(builder.DataSource);
        if (!match.Success)
        {
            return connectionString;
        }

        var instance = match.Groups["instance"].Value;
        try
        {
            var pipe = GetPipeName(instance) ?? StartAndGetPipeName(instance);
            if (pipe is null)
            {
                logger.LogWarning("LocalDB instance {Instance} isn't running and couldn't be started; using {DataSource} as-is. " +
                                  "Start it with `sqllocaldb start {Instance}` and restart the app.", instance, builder.DataSource, instance);
                return connectionString;
            }

            builder.DataSource = pipe;
            logger.LogInformation("Connecting to LocalDB instance {Instance} via named pipe {Pipe}", instance, pipe);
            return builder.ConnectionString;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't resolve the LocalDB pipe for {Instance}; using the connection string as-is.", instance);
            return connectionString;
        }
    }

    private static string? StartAndGetPipeName(string instance)
    {
        RunSqlLocalDb($"start \"{instance}\"");
        return GetPipeName(instance);
    }

    private static string? GetPipeName(string instance)
    {
        var output = RunSqlLocalDb($"info \"{instance}\"");
        var pipe = PipeLine().Match(output);
        return pipe.Success ? pipe.Groups["pipe"].Value.Trim() : null;
    }

    private static string RunSqlLocalDb(string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo("SqlLocalDB.exe", arguments)
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
}
