using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicWebsiteConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicWebsiteConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgencyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Slogan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HeroTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HeroSubtitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HeroBannerUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AboutTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AboutText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContactWhatsapp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InstagramUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FacebookUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SeoTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SeoDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PrimaryColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AccentColor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicWebsiteConfig", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicWebsiteConfig");
        }
    }
}
