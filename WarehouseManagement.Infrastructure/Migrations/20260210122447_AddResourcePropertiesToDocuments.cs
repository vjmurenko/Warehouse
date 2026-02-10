using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcePropertiesToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShipmentResources_ResourceId",
                table: "ShipmentResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentResources_UnitOfMeasureId",
                table: "ShipmentResources",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentDocuments_ClientId",
                table: "ShipmentDocuments",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptResources_ResourceId",
                table: "ReceiptResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptResources_UnitOfMeasureId",
                table: "ReceiptResources",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_UnitOfMeasureId",
                table: "Balances",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Resources_ResourceId",
                table: "Balances",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_UnitsOfMeasure_UnitOfMeasureId",
                table: "Balances",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptResources_Resources_ResourceId",
                table: "ReceiptResources",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptResources_UnitsOfMeasure_UnitOfMeasureId",
                table: "ReceiptResources",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentDocuments_Clients_ClientId",
                table: "ShipmentDocuments",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentResources_Resources_ResourceId",
                table: "ShipmentResources",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentResources_UnitsOfMeasure_UnitOfMeasureId",
                table: "ShipmentResources",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Resources_ResourceId",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Balances_UnitsOfMeasure_UnitOfMeasureId",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptResources_Resources_ResourceId",
                table: "ReceiptResources");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptResources_UnitsOfMeasure_UnitOfMeasureId",
                table: "ReceiptResources");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentDocuments_Clients_ClientId",
                table: "ShipmentDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentResources_Resources_ResourceId",
                table: "ShipmentResources");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentResources_UnitsOfMeasure_UnitOfMeasureId",
                table: "ShipmentResources");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentResources_ResourceId",
                table: "ShipmentResources");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentResources_UnitOfMeasureId",
                table: "ShipmentResources");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentDocuments_ClientId",
                table: "ShipmentDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptResources_ResourceId",
                table: "ReceiptResources");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptResources_UnitOfMeasureId",
                table: "ReceiptResources");

            migrationBuilder.DropIndex(
                name: "IX_Balances_UnitOfMeasureId",
                table: "Balances");
        }
    }
}
