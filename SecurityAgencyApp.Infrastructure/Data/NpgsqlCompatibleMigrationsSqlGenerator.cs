using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace SecurityAgencyApp.Infrastructure.Data;

/// <summary>
/// Maps SQL Server migration types to PostgreSQL so the same migration files work for both providers.
/// </summary>
public class NpgsqlCompatibleMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    public NpgsqlCompatibleMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions)
        : base(dependencies, npgsqlSingletonOptions)
    {
    }

    protected override void Generate(MigrationOperation operation, Microsoft.EntityFrameworkCore.Metadata.IModel? model, MigrationCommandListBuilder builder)
    {
        MapSqlServerTypesToPostgres(operation);
        base.Generate(operation, model, builder);
    }

    private static void MapSqlServerTypesToPostgres(MigrationOperation operation)
    {
        switch (operation)
        {
            case CreateTableOperation createTable:
                foreach (var column in createTable.Columns)
                    column.ColumnType = MapType(column.ColumnType);
                break;
            case AddColumnOperation addColumn:
                addColumn.ColumnType = MapType(addColumn.ColumnType);
                break;
            case AlterColumnOperation alterColumn:
                alterColumn.ColumnType = MapType(alterColumn.ColumnType);
                break;
        }
    }

    private static string? MapType(string? sqlServerType)
    {
        if (string.IsNullOrWhiteSpace(sqlServerType)) return sqlServerType;

        var t = sqlServerType.Trim();

        if (string.Equals(t, "uniqueidentifier", StringComparison.OrdinalIgnoreCase))
            return "uuid";
        if (string.Equals(t, "datetime2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(t, "datetime", StringComparison.OrdinalIgnoreCase))
            return "timestamp without time zone";
        if (string.Equals(t, "date", StringComparison.OrdinalIgnoreCase))
            return "date";
        if (string.Equals(t, "time", StringComparison.OrdinalIgnoreCase))
            return "time without time zone";
        if (string.Equals(t, "bit", StringComparison.OrdinalIgnoreCase))
            return "boolean";
        if (string.Equals(t, "int", StringComparison.OrdinalIgnoreCase))
            return "integer";
        if (string.Equals(t, "bigint", StringComparison.OrdinalIgnoreCase))
            return "bigint";
        if (string.Equals(t, "smallint", StringComparison.OrdinalIgnoreCase))
            return "smallint";
        if (string.Equals(t, "tinyint", StringComparison.OrdinalIgnoreCase))
            return "smallint";
        if (string.Equals(t, "float", StringComparison.OrdinalIgnoreCase))
            return "double precision";
        if (string.Equals(t, "real", StringComparison.OrdinalIgnoreCase))
            return "real";
        if (string.Equals(t, "nvarchar(max)", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(t, "varchar(max)", StringComparison.OrdinalIgnoreCase))
            return "text";

        var nvarcharMatch = Regex.Match(t, @"^nvarchar\((\d+)\)$", RegexOptions.IgnoreCase);
        if (nvarcharMatch.Success)
            return $"character varying({nvarcharMatch.Groups[1].Value})";
        var varcharMatch = Regex.Match(t, @"^varchar\((\d+)\)$", RegexOptions.IgnoreCase);
        if (varcharMatch.Success)
            return $"character varying({varcharMatch.Groups[1].Value})";

        var decimalMatch = Regex.Match(t, @"^decimal\((\d+),\s*(\d+)\)$", RegexOptions.IgnoreCase);
        if (decimalMatch.Success)
            return $"numeric({decimalMatch.Groups[1].Value},{decimalMatch.Groups[2].Value})";
        var numericMatch = Regex.Match(t, @"^numeric\((\d+),\s*(\d+)\)$", RegexOptions.IgnoreCase);
        if (numericMatch.Success)
            return $"numeric({numericMatch.Groups[1].Value},{numericMatch.Groups[2].Value})";

        return t;
    }
}
