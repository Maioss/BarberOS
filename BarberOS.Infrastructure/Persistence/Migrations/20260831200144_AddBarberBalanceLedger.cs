using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBarberBalanceLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "barber_balance_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barber_balance_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_barber_balance_entries_barbers_BarberId",
                        column: x => x.BarberId,
                        principalTable: "barbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_barber_balance_entries_AppointmentId_Reason",
                table: "barber_balance_entries",
                columns: new[] { "AppointmentId", "Reason" },
                unique: true,
                filter: "\"AppointmentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_barber_balance_entries_BarberId",
                table: "barber_balance_entries",
                column: "BarberId");

            migrationBuilder.CreateIndex(
                name: "IX_barber_balance_entries_PaymentId_Reason",
                table: "barber_balance_entries",
                columns: new[] { "PaymentId", "Reason" },
                unique: true,
                filter: "\"PaymentId\" IS NOT NULL");

            // El saldo que ya tenia cada barbero pasa a ser su asiento de apertura,
            // para que borrar la columna no pierda dinero.
            migrationBuilder.Sql("""
                INSERT INTO barber_balance_entries
                    ("Id", "BarberId", "Amount", "Reason", "AppointmentId", "PaymentId", "CreatedAt")
                SELECT gen_random_uuid(), "Id", "Balance", 3, NULL, NULL, now() AT TIME ZONE 'utc'
                FROM barbers
                WHERE "Balance" <> 0;
                """);

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "barbers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "barber_balance_entries");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "barbers",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
