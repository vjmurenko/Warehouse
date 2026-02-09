using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStockMovementWithBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Balances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Balances_ResourceId_UnitOfMeasureId",
                table: "Balances",
                columns: new[] { "ResourceId", "UnitOfMeasureId" },
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""Balances"" (""Id"", ""ResourceId"", ""UnitOfMeasureId"", ""Quantity"")
                SELECT 
                    gen_random_uuid(),
                    ""ResourceId"",
                    ""UnitOfMeasureId"",
                    SUM(""Quantity"")
                FROM ""StockMovements""
                GROUP BY ""ResourceId"", ""UnitOfMeasureId""
                HAVING SUM(""Quantity"") > 0
            ");

            migrationBuilder.DropTable(
                name: "StockMovements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Balances");

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_DocumentId",
                table: "StockMovements",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ResourceId_UnitOfMeasureId",
                table: "StockMovements",
                columns: new[] { "ResourceId", "UnitOfMeasureId" });
        }
    }
}
