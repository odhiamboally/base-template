const fs = require('fs');
const path1 = 'E:\\Repos\\BaseTemplate\\src\\Backend\\Persistence\\BT.Persistence\\Features\\Shared\\Migrations\\SqlServer\\20260724082103_QuartzInitialSchema.cs';
const path2 = 'E:\\Repos\\BaseTemplate\\src\\Backend\\Persistence\\BT.Persistence\\Features\\Shared\\Migrations\\PostgreSql\\20260724082144_QuartzInitialSchema.cs';
const sql1 = fs.readFileSync(process.env.TEMP + '\\tables_sqlServer.sql', 'utf8');
const sql2 = fs.readFileSync(process.env.TEMP + '\\tables_postgres.sql', 'utf8');

let content1 = fs.readFileSync(path1, 'utf8');
content1 = content1.replace(/protected override void Up\(MigrationBuilder migrationBuilder\)\s*\{\s*\}/, "protected override void Up(MigrationBuilder migrationBuilder)\n        {\n            migrationBuilder.Sql(\"\"\"\n" + sql1 + "\n\"\"\");\n        }");
fs.writeFileSync(path1, content1);

let content2 = fs.readFileSync(path2, 'utf8');
content2 = content2.replace(/protected override void Up\(MigrationBuilder migrationBuilder\)\s*\{\s*\}/, "protected override void Up(MigrationBuilder migrationBuilder)\n        {\n            migrationBuilder.Sql(\"\"\"\n" + sql2 + "\n\"\"\");\n        }");
fs.writeFileSync(path2, content2);
console.log('done');
