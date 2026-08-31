using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberOS.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Impide que dos citas confirmadas del mismo barbero se crucen. La comprobacion
    /// en codigo consultaba y luego insertaba, con nada en medio: ocho peticiones
    /// simultaneas creaban ocho citas en el mismo hueco. Un indice unico sobre la hora
    /// de inicio no bastaba, porque un servicio de 45 minutos se solapa con el que
    /// empieza media hora despues; por eso la restriccion compara rangos.
    /// </summary>
    public partial class PreventOverlappingAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // btree_gist permite mezclar la igualdad de BarberId con el solapamiento
            // de rangos dentro de la misma restriccion.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // Solo aplica a las confirmadas: canceladas y completadas pueden convivir
            // en el mismo horario sin problema.
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
