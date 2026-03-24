using System.Data;
using Microsoft.EntityFrameworkCore;

namespace AccidentBook.API.Data;

/// <summary>
/// Ensures SQLite schema matches the current model and applies registration columns to older databases.
/// </summary>
public static class SqliteRegistrationSchema
{
    /// <summary>
    /// Creates the database if needed. If the file existed from an older app version without a Users table,
    /// <see cref="DatabaseFacade.EnsureCreated"/> is a no-op; we then delete and recreate so all tables exist.
    /// </summary>
    public static void EnsureDatabaseIsReady(AccidentDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Database.IsSqlite())
            return;

        if (!UsersTableExists(context))
        {
            // Old DB file (e.g. only Accidents) — EnsureCreated does not add new tables
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        ApplyIfNeeded(context);
    }

    private static bool UsersTableExists(AccidentDbContext context)
    {
        var conn = context.Database.GetDbConnection();
        var wasOpen = conn.State == ConnectionState.Open;
        if (!wasOpen)
            conn.Open();
        try
        {
            return TableExists(conn, "Users");
        }
        finally
        {
            if (!wasOpen)
                conn.Close();
        }
    }

    public static void ApplyIfNeeded(AccidentDbContext context)
    {
        if (!context.Database.IsSqlite())
            return;

        var conn = context.Database.GetDbConnection();
        if (conn.DataSource == null)
            return;

        {
            var wasOpen = conn.State == ConnectionState.Open;
            if (!wasOpen)
                conn.Open();
            try
            {
                if (!TableExists(conn, "Users"))
                    return;

                foreach (var (name, ddl) in ColumnsToAdd)
                {
                    if (!ColumnExists(conn, "Users", name))
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE Users ADD COLUMN {name} {ddl}";
                        cmd.ExecuteNonQuery();
                    }
                }

                EnsureUniqueIndexOnPendingApprovalToken(context);
            }
            finally
            {
                if (!wasOpen)
                    conn.Close();
            }
        }
    }

    private static readonly (string Name, string Ddl)[] ColumnsToAdd =
    {
        ("EmailVerified", "INTEGER NOT NULL DEFAULT 1"),
        ("AdminApproved", "INTEGER NOT NULL DEFAULT 1"),
        ("EmailVerificationCodeHash", "TEXT NULL"),
        ("EmailVerificationExpiresAt", "TEXT NULL"),
        ("PendingApprovalToken", "TEXT NULL"),
    };

    private static bool TableExists(System.Data.Common.DbConnection conn, string table)
    {
        using var cmd = conn.CreateCommand();
        // Table names are internal constants only; quote for SQLite.
        cmd.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='" +
            table.Replace("'", "''", StringComparison.Ordinal) +
            "';";
        var result = cmd.ExecuteScalar();
        return result is long l && l > 0;
    }

    private static bool ColumnExists(System.Data.Common.DbConnection conn, string table, string column)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='{column}'";
        var result = cmd.ExecuteScalar();
        return result is long l && l > 0;
    }

    private static void EnsureUniqueIndexOnPendingApprovalToken(AccidentDbContext context)
    {
        try
        {
            context.Database.ExecuteSqlRaw(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_PendingApprovalToken ON Users (PendingApprovalToken)");
        }
        catch
        {
            // Ignore if SQLite version or state prevents index creation
        }
    }
}
