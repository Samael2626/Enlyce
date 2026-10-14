using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Enlyce.Infrastructure.Migrations;

[DbContext(typeof(EnlyceDbContext))]
[Migration("20261014180000_AddCustomerLabels")]
public sealed class AddCustomerLabels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<string[]>(
            name: "CustomerLabels",
            table: "Contacts",
            type: "text[]",
            nullable: false,
            defaultValueSql: "ARRAY[]::text[]");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "CustomerLabels", table: "Contacts");
}
