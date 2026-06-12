using BT.Persistence.Features.HR.DataContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BT.Persistence.Migrations.HR
{
    [DbContext(typeof(HrDBContext))]
    [Migration("20260611143000_ReAlignHrTenantScope")]
    public partial class ReAlignHrTenantScope : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Departments
                SET TenantId = '0194f700-0000-7000-8000-000000000001'
                WHERE TenantId = '00000000-0000-0000-0000-000000000000';

                UPDATE Employees
                SET TenantId = '0194f700-0000-7000-8000-000000000001'
                WHERE TenantId = '00000000-0000-0000-0000-000000000000';

                UPDATE EmployeeNumberSequences
                SET TenantId = '0194f700-0000-7000-8000-000000000001'
                WHERE TenantId = '00000000-0000-0000-0000-000000000000';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
