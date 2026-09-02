using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameBarbershopsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_Barbershops_BarbershopId",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_barbers_Barbershops_BarbershopId",
                table: "barbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Barbershops_Barbershops_ParentId",
                table: "Barbershops");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_Barbershops_BarbershopId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_services_Barbershops_BarbershopId",
                table: "services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Barbershops",
                table: "Barbershops");

            migrationBuilder.RenameTable(
                name: "Barbershops",
                newName: "barbershops");

            migrationBuilder.RenameIndex(
                name: "IX_Barbershops_ParentId",
                table: "barbershops",
                newName: "IX_barbershops_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_barbershops",
                table: "barbershops",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_barbershops_BarbershopId",
                table: "appointments",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_barbers_barbershops_BarbershopId",
                table: "barbers",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_barbershops_barbershops_ParentId",
                table: "barbershops",
                column: "ParentId",
                principalTable: "barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_barbershops_BarbershopId",
                table: "payments",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_services_barbershops_BarbershopId",
                table: "services",
                column: "BarbershopId",
                principalTable: "barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_appointments_barbershops_BarbershopId",
                table: "appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_barbers_barbershops_BarbershopId",
                table: "barbers");

            migrationBuilder.DropForeignKey(
                name: "FK_barbershops_barbershops_ParentId",
                table: "barbershops");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_barbershops_BarbershopId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_services_barbershops_BarbershopId",
                table: "services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_barbershops",
                table: "barbershops");

            migrationBuilder.RenameTable(
                name: "barbershops",
                newName: "Barbershops");

            migrationBuilder.RenameIndex(
                name: "IX_barbershops_ParentId",
                table: "Barbershops",
                newName: "IX_Barbershops_ParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Barbershops",
                table: "Barbershops",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_appointments_Barbershops_BarbershopId",
                table: "appointments",
                column: "BarbershopId",
                principalTable: "Barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_barbers_Barbershops_BarbershopId",
                table: "barbers",
                column: "BarbershopId",
                principalTable: "Barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Barbershops_Barbershops_ParentId",
                table: "Barbershops",
                column: "ParentId",
                principalTable: "Barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_Barbershops_BarbershopId",
                table: "payments",
                column: "BarbershopId",
                principalTable: "Barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_services_Barbershops_BarbershopId",
                table: "services",
                column: "BarbershopId",
                principalTable: "Barbershops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
