using Microsoft.Extensions.Logging;
using Npgsql;

namespace KinkLinkCommon.Database;

/// <summary>
/// Manages database schema migrations
/// </summary>
public class MigrationManager
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<MigrationManager> _logger;
    private const string MigrationTable = "SchemaMigrations";

    public MigrationManager(NpgsqlConnection connection, ILogger<MigrationManager> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    /// <summary>
    /// Ensures migration tracking table exists and runs pending migrations
    /// </summary>
    public async Task RunMigrationsAsync(IEnumerable<string> migrationFiles)
    {
        await EnsureMigrationTableExistsAsync();

        var appliedMigrations = await GetAppliedMigrationsAsync();
        var migrationsToApply = migrationFiles
            .Where(f => !appliedMigrations.Contains(GetMigrationVersion(f)))
            .OrderBy(GetMigrationVersion)
            .ToList();

        if (migrationsToApply.Count == 0)
        {
            _logger.LogInformation("Database is up to date");
            return;
        }

        _logger.LogInformation("Running {Count} migrations...", migrationsToApply.Count);

        foreach (var migrationFile in migrationsToApply)
        {
            await RunMigrationAsync(migrationFile);
        }
    }

    private async Task EnsureMigrationTableExistsAsync()
    {
        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {MigrationTable} (
                Version VARCHAR(10) PRIMARY KEY,
                AppliedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
            )";

        await using var cmd = new NpgsqlCommand(createTableSql, _connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<List<string>> GetAppliedMigrationsAsync()
    {
        var migrations = new List<string>();

        await using var cmd = new NpgsqlCommand(
            $"SELECT Version FROM {MigrationTable}", _connection);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            migrations.Add(reader.GetString(0));
        }

        return migrations;
    }

    private async Task RunMigrationAsync(string filePath)
    {
        var version = GetMigrationVersion(filePath);
        var sql = await File.ReadAllTextAsync(filePath);

        await using var transaction = await _connection.BeginTransactionAsync();

        try
        {
            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Transaction = transaction as NpgsqlTransaction;
            await cmd.ExecuteNonQueryAsync();

            // Record migration
            var insertSql = $"INSERT INTO {MigrationTable} (Version) VALUES (@version)";
            await using var insertCmd = new NpgsqlCommand(insertSql, _connection);
            insertCmd.Transaction = transaction as NpgsqlTransaction;
            insertCmd.Parameters.AddWithValue("@version", version);
            await insertCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("Migration {Version} applied successfully", version);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Migration {Version} failed", version);
            throw;
        }
    }

    private static string GetMigrationVersion(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return fileName.Split('_')[0]; // Returns "001" from "001_create_tables.sql"
    }

    /// <summary>
    /// Gets all migration file paths from the migrations directory
    /// </summary>
    public static IEnumerable<string> GetMigrationFiles(string migrationsPath)
    {
        if (!Directory.Exists(migrationsPath))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(migrationsPath, "*.sql")
            .OrderBy(f => GetMigrationVersion(f));
    }
}
