using Enlyce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations;

[DbContext(typeof(EnlyceDbContext))]
[Migration("20261014162300_AddDemandAndPropertyFeatures")]
public sealed class AddDemandAndPropertyFeatures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string[]>("InternalFeatures", "CustomerDemands", type: "text[]", nullable: false, defaultValueSql: "ARRAY[]::text[]");
        migrationBuilder.AddColumn<string[]>("ExternalFeatures", "CustomerDemands", type: "text[]", nullable: false, defaultValueSql: "ARRAY[]::text[]");
        migrationBuilder.AddColumn<string[]>("InternalFeatures", "Inmuebles", type: "text[]", nullable: false, defaultValueSql: "ARRAY[]::text[]");
        migrationBuilder.AddColumn<string[]>("ExternalFeatures", "Inmuebles", type: "text[]", nullable: false, defaultValueSql: "ARRAY[]::text[]");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("InternalFeatures", "CustomerDemands");
        migrationBuilder.DropColumn("ExternalFeatures", "CustomerDemands");
        migrationBuilder.DropColumn("InternalFeatures", "Inmuebles");
        migrationBuilder.DropColumn("ExternalFeatures", "Inmuebles");
    }
}
