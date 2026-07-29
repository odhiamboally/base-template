using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        var cs = ""Server=tcp:btprod-sql-34m7iy63gdyt6.database.windows.net,1433;Initial Catalog=BT;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;"";
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(cs);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = ""SELECT name, type_desc FROM sys.database_principals WHERE type_desc IN ('EXTERNAL_USER', 'EXTERNAL_GROUPS')"";
        using var reader = cmd.ExecuteReader();
        Console.WriteLine(""Users in DB:"");
        while(reader.Read()) {
            Console.WriteLine(reader.GetString(0) + "" "" + reader.GetString(1));
        }
    }
}
