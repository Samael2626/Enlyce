using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Enlyce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TrackPaymentOrderStatusChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentOrderStatusChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ProviderTransactionId = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrderStatusChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrderStatusChanges_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderStatusChanges_PaymentOrderId_OccurredAt",
                table: "PaymentOrderStatusChanges",
                columns: new[] { "PaymentOrderId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentOrderStatusChanges");
        }
    }
}
