using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

using Microsoft.EntityFrameworkCore.Update;

namespace BT.Persistence.Features.Shared.Migrations.Generators;

public class IdempotentSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
{
#pragma warning disable EF1001 // Internal EF Core API usage.
    public IdempotentSqlServerMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        ICommandBatchPreparer commandBatchPreparer)
        : base(dependencies, commandBatchPreparer)
    {
    }
#pragma warning restore EF1001 // Internal EF Core API usage.

    protected override void Generate(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        var schema = operation.Schema;
        var table = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, schema);
        
        builder
            .Append($"IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = '{operation.Name}' AND object_id = OBJECT_ID('{table}'))")
            .AppendLine()
            .Append("BEGIN")
            .AppendLine();

        using (builder.Indent())
        {
            base.Generate(operation, model, builder, terminate: false);
            builder.AppendLine(";");
        }

        builder
            .Append("END")
            .AppendLine()
            .Append("ELSE")
            .AppendLine()
            .Append("BEGIN")
            .AppendLine()
            .Append($"    PRINT 'Index {operation.Name} on table {table} already exists. Skipping creation.';")
            .AppendLine()
            .Append("END")
            .AppendLine();

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            builder.EndCommand();
        }
    }
}
