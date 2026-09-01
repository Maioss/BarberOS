using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;
using static BarberOS.Domain.Tests.TestData;

namespace BarberOS.Domain.Tests;

public class AppointmentTests
{
    [Fact]
    public void Create_calcula_el_fin_sumando_la_duracion_de_los_servicios()
    {
        var appointment = NewAppointment(
            start: new TimeOnly(9, 0),
            services: [NewService(minutes: 30), NewService(minutes: 45)]);

        Assert.Equal(new TimeOnly(10, 15), appointment.EndTime);
    }

    [Fact]
    public void Create_suma_el_precio_de_los_servicios()
    {
        var appointment = NewAppointment(
            services: [NewService(price: 25000m), NewService(price: 15000m)]);

        Assert.Equal(40000m, appointment.TotalPrice);
    }

    [Fact]
    public void Create_copia_los_datos_del_servicio_para_que_un_cambio_de_precio_no_reescriba_el_historial()
    {
        var service = NewService(name: "Corte", price: 25000m, minutes: 30);
        var appointment = NewAppointment(services: [service]);

        service.Update("Corte premium", null, 90000m, 60);

        var snapshot = Assert.Single(appointment.Services);
        Assert.Equal("Corte", snapshot.ServiceName);
        Assert.Equal(25000m, snapshot.Price);
        Assert.Equal(30, snapshot.DurationMinutes);
        Assert.Equal(25000m, appointment.TotalPrice);
    }

    [Fact]
    public void Create_exige_al_menos_un_servicio()
    {
        var ex = Assert.Throws<BusinessRuleException>(() =>
            Entities.Appointment.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Today, new TimeOnly(10, 0), []));

        Assert.Contains("al menos un servicio", ex.Message);
    }

    [Fact]
    public void Create_nace_confirmada()
    {
        Assert.Equal(AppointmentStatus.Confirmed, NewAppointment().Status);
    }

    [Fact]
    public void Complete_no_admite_una_cita_futura()
    {
        var appointment = NewAppointment(date: Today.AddDays(1));

        Assert.Throws<BusinessRuleException>(() => appointment.Complete(Today));
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
    }

    [Fact]
    public void Complete_admite_la_cita_de_hoy()
    {
        var appointment = NewAppointment(date: Today);

        appointment.Complete(Today);

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        Assert.NotNull(appointment.CompletedAt);
    }

    [Fact]
    public void Complete_dos_veces_es_conflicto()
    {
        var appointment = NewAppointment();
        appointment.Complete(Today);

        Assert.Throws<ConflictException>(() => appointment.Complete(Today));
    }

    [Fact]
    public void Complete_no_admite_una_cita_cancelada()
    {
        var appointment = NewAppointment();
        appointment.Cancel();

        Assert.Throws<BusinessRuleException>(() => appointment.Complete(Today));
    }

    [Fact]
    public void Cancel_dos_veces_es_conflicto()
    {
        var appointment = NewAppointment();
        appointment.Cancel();

        Assert.Throws<ConflictException>(() => appointment.Cancel());
    }

    [Fact]
    public void Cancel_no_admite_una_cita_completada()
    {
        var appointment = NewAppointment();
        appointment.Complete(Today);

        Assert.Throws<BusinessRuleException>(() => appointment.Cancel());
    }

    private static bool CitaDeDiezATreinta_SeSolapaCon(TimeOnly start, TimeOnly end) =>
        NewAppointment(start: new TimeOnly(10, 0), services: [NewService(minutes: 30)])
            .OverlapsWith(start, end);

    [Fact]
    public void OverlapsWith_no_ve_solape_en_la_que_termina_justo_cuando_esta_empieza()
    {
        Assert.False(CitaDeDiezATreinta_SeSolapaCon(new TimeOnly(9, 30), new TimeOnly(10, 0)));
    }

    [Fact]
    public void OverlapsWith_no_ve_solape_en_la_que_empieza_justo_cuando_esta_termina()
    {
        Assert.False(CitaDeDiezATreinta_SeSolapaCon(new TimeOnly(10, 30), new TimeOnly(11, 0)));
    }

    [Fact]
    public void OverlapsWith_ve_solape_en_la_que_se_monta_sobre_el_arranque()
    {
        Assert.True(CitaDeDiezATreinta_SeSolapaCon(new TimeOnly(9, 45), new TimeOnly(10, 15)));
    }

    [Fact]
    public void OverlapsWith_ve_solape_en_la_que_se_monta_sobre_el_final()
    {
        Assert.True(CitaDeDiezATreinta_SeSolapaCon(new TimeOnly(10, 15), new TimeOnly(10, 45)));
    }

    [Fact]
    public void OverlapsWith_ve_solape_en_la_que_la_contiene()
    {
        Assert.True(CitaDeDiezATreinta_SeSolapaCon(new TimeOnly(9, 0), new TimeOnly(12, 0)));
    }

    [Fact]
    public void OverlapsWith_ve_solape_en_la_que_cae_dentro()
    {
        Assert.True(CitaDeDiezATreinta_SeSolapaCon(new TimeOnly(10, 10), new TimeOnly(10, 20)));
    }
}
