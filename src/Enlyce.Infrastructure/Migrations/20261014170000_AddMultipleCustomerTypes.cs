using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Enlyce.Infrastructure.Migrations;

[DbContext(typeof(EnlyceDbContext))]
[Migration("20261014170000_AddMultipleCustomerTypes")]
public sealed class AddMultipleCustomerTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string[]>(
            name: "CustomerTypes",
            table: "Contacts",
            type: "text[]",
            nullable: false,
            defaultValueSql: "ARRAY[]::text[]");

        migrationBuilder.Sql("UPDATE \"Contacts\" SET \"CustomerTypes\" = ARRAY[\"CustomerType\"] WHERE \"CustomerType\" IS NOT NULL AND \"CustomerType\" <> '';");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "CustomerTypes", table: "Contacts");
}
