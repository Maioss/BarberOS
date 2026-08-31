using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberOS.Infrastructure.Persistence.Migrations
{
    public partial class PreventOverlappingAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Un indice unico sobre StartTime no basta: un servicio de 45 minutos se
            // solapa con el que empieza media hora despues. De ahi el rango y btree_gist.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                ALTER TABLE appointments
                ADD CONSTRAINT appointments_no_overlap
                EXCLUDE USING gist (
                    "BarberId" WITH =,
                    tsrange("Date" + "StartTime", "Date" + "EndTime") WITH &&
                )
                WHERE ("Status" = 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE appointments DROP CONSTRAINT IF EXISTS appointments_no_overlap;");
        }
    }
}
