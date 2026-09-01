using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBarbershopTimeZoneAndRelativePhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Barbershops",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "America/Bogota");

            migrationBuilder.Sql("""
                UPDATE users
                SET "PhotoUrl" = regexp_replace("PhotoUrl", '^https?://[^/]+', '')
                WHERE "PhotoUrl" ~ '^https?://[^/]+/photos/';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Barbershops");
        }
    }
}
