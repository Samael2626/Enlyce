using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Enlyce.Infrastructure.Migrations;

[DbContext(typeof(EnlyceDbContext))]
[Migration("20261014160000_AddContactFunnel")]
public sealed class AddContactFunnel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<string>(
            name: "CustomerFunnel",
            table: "Contacts",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "Clientes");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "CustomerFunnel", table: "Contacts");
}
