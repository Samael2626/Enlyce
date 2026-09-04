using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyPublications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyPublications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PublicTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PublicDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PublicPriceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PublicPriceCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Municipality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApproximateLatitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ApproximateLongitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    ExactAddressVisible = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyPublications_Asesores_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "Asesores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyPublications_Inmuebles_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Inmuebles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropertyPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    AltText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsCover = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyPhotos_PropertyPublications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "PropertyPublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inmuebles_Modalidad",
                table: "Inmuebles",
                column: "Modalidad");

            migrationBuilder.CreateIndex(
                name: "IX_Inmuebles_Tipo",
                table: "Inmuebles",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPhotos_PublicationId_Order",
                table: "PropertyPhotos",
                columns: new[] { "PublicationId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_AdvisorId",
                table: "PropertyPublications",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_Municipality",
                table: "PropertyPublications",
                column: "Municipality");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_Neighborhood",
                table: "PropertyPublications",
                column: "Neighborhood");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_PropertyId",
                table: "PropertyPublications",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_PublicPriceAmount",
                table: "PropertyPublications",
                column: "PublicPriceAmount");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_PublishedAt",
                table: "PropertyPublications",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_Slug",
                table: "PropertyPublications",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_Status",
                table: "PropertyPublications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyPhotos");

            migrationBuilder.DropTable(
                name: "PropertyPublications");

            migrationBuilder.DropIndex(
                name: "IX_Inmuebles_Modalidad",
                table: "Inmuebles");

            migrationBuilder.DropIndex(
                name: "IX_Inmuebles_Tipo",
                table: "Inmuebles");
        }
    }
}
