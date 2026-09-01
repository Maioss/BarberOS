using BarberOS.Domain.Exceptions;
using static BarberOS.Domain.Tests.TestData;

namespace BarberOS.Domain.Tests;

public class BarberTests
{
    [Fact]
    public void Create_arranca_trabajando_de_lunes_a_sabado()
    {
        var barber = NewBarber();

        Assert.Equal(
            new[]
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
            },
            barber.GetAvailableDays());

        Assert.False(barber.IsAvailableOn(DayOfWeek.Sunday));
    }

    [Fact]
    public void UpdateSchedule_conserva_los_dias_que_se_le_pasan()
    {
        var barber = NewBarber();
        DayOfWeek[] days = [DayOfWeek.Sunday, DayOfWeek.Wednesday];

        barber.UpdateSchedule(new TimeOnly(13, 0), new TimeOnly(14, 0), days);

        Assert.Equal(days.Order(), barber.GetAvailableDays().Order());
        Assert.True(barber.IsAvailableOn(DayOfWeek.Sunday));
        Assert.False(barber.IsAvailableOn(DayOfWeek.Monday));
    }

    [Fact]
    public void UpdateSchedule_exige_que_el_almuerzo_empiece_antes_de_terminar()
    {
        var barber = NewBarber();

        Assert.Throws<BusinessRuleException>(() =>
            barber.UpdateSchedule(new TimeOnly(14, 0), new TimeOnly(13, 0), [DayOfWeek.Monday]));
    }

    [Fact]
    public void UpdateSchedule_rechaza_un_almuerzo_de_duracion_cero()
    {
        var barber = NewBarber();

        Assert.Throws<BusinessRuleException>(() =>
            barber.UpdateSchedule(new TimeOnly(13, 0), new TimeOnly(13, 0), [DayOfWeek.Monday]));
    }

    [Theory]
    [InlineData(8, 0, 9, 0)]    // empieza antes de abrir
    [InlineData(17, 30, 18, 30)] // termina despues de cerrar
    public void UpdateSchedule_exige_que_el_almuerzo_caiga_en_horario_laboral(
        int startHour, int startMinute, int endHour, int endMinute)
    {
        var barber = NewBarber();

        Assert.Throws<BusinessRuleException>(() =>
            barber.UpdateSchedule(
                new TimeOnly(startHour, startMinute),
                new TimeOnly(endHour, endMinute),
                [DayOfWeek.Monday]));
    }

    [Fact]
    public void UpdateSchedule_exige_al_menos_un_dia()
    {
        var barber = NewBarber();

        Assert.Throws<BusinessRuleException>(() =>
            barber.UpdateSchedule(new TimeOnly(13, 0), new TimeOnly(14, 0), []));
    }

    [Fact]
    public void UpdateSchedule_ignora_los_dias_repetidos()
    {
        var barber = NewBarber();

        barber.UpdateSchedule(
            new TimeOnly(13, 0), new TimeOnly(14, 0),
            [DayOfWeek.Monday, DayOfWeek.Monday, DayOfWeek.Friday]);

        Assert.Equal(2, barber.GetAvailableDays().Count);
    }

    [Fact]
    public void Deactivate_apaga_al_barbero()
    {
        var barber = NewBarber();

        barber.Deactivate();

        Assert.False(barber.IsActive);
    }
}
