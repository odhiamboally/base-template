using System;
using System.IO;

class Program {
    static void Main() {
        Process(@\"E:\Repos\BaseTemplate\src\Backend\Persistence\BT.Persistence\Features\Shared\Migrations\SqlServer\20260724082103_QuartzInitialSchema.cs\", Environment.GetEnvironmentVariable(\"TEMP\") + @\"\tables_sqlServer.sql\");
        Process(@\"E:\Repos\BaseTemplate\src\Backend\Persistence\BT.Persistence\Features\Shared\Migrations\PostgreSql\20260724082144_QuartzInitialSchema.cs\", Environment.GetEnvironmentVariable(\"TEMP\") + @\"\tables_postgres.sql\");
    }

    static void Process(string csPath, string sqlPath) {
        var sql = File.ReadAllText(sqlPath);
        var cs = File.ReadAllText(csPath);
        var replacement = \"protected override void Up(MigrationBuilder migrationBuilder)\\r\\n        {\\r\\n            migrationBuilder.Sql(\\\"\\\"\\\"\\r\\n\" + sql + \"\\r\\n\\\"\\\"\\\");\\r\\n        }\";
        cs = System.Text.RegularExpressions.Regex.Replace(cs, @\"protected override void Up\\(MigrationBuilder migrationBuilder\\)\\s*\\{\\s*\\}\", replacement);
        File.WriteAllText(csPath, cs);
    }
}
