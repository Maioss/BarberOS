using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Tests;

public class BalanceEntryTests
{
    private static readonly Guid BarberId = Guid.NewGuid();

    [Fact]
    public void ForCompletedAppointment_acredita_en_positivo()
    {
        var appointmentId = Guid.NewGuid();

        var entry = BalanceEntry.ForCompletedAppointment(BarberId, appointmentId, 25000m);

        Assert.Equal(25000m, entry.Amount);
        Assert.Equal(BalanceEntryReason.AppointmentCompleted, entry.Reason);
        Assert.Equal(appointmentId, entry.AppointmentId);
        Assert.Null(entry.PaymentId);
    }

    [Fact]
    public void ForRefundedPayment_descuenta_en_negativo()
    {
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var entry = BalanceEntry.ForRefundedPayment(BarberId, paymentId, appointmentId, 25000m);

        Assert.Equal(-25000m, entry.Amount);
        Assert.Equal(BalanceEntryReason.PaymentRefunded, entry.Reason);
        Assert.Equal(paymentId, entry.PaymentId);
        Assert.Equal(appointmentId, entry.AppointmentId);
    }

    [Fact]
    public void Completar_y_reembolsar_lo_mismo_deja_el_saldo_en_cero()
    {
        var appointmentId = Guid.NewGuid();

        var credit = BalanceEntry.ForCompletedAppointment(BarberId, appointmentId, 25000m);
        var debit = BalanceEntry.ForRefundedPayment(BarberId, Guid.NewGuid(), appointmentId, 25000m);

        Assert.Equal(0m, credit.Amount + debit.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ForCompletedAppointment_exige_un_monto_positivo(decimal amount)
    {
        Assert.Throws<BusinessRuleException>(() =>
            BalanceEntry.ForCompletedAppointment(BarberId, Guid.NewGuid(), amount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ForRefundedPayment_exige_un_monto_positivo(decimal amount)
    {
        Assert.Throws<BusinessRuleException>(() =>
            BalanceEntry.ForRefundedPayment(BarberId, Guid.NewGuid(), Guid.NewGuid(), amount));
    }

    [Fact]
    public void Un_movimiento_de_cero_no_tiene_sentido()
    {
        Assert.Throws<BusinessRuleException>(() => BalanceEntry.ForAdjustment(BarberId, 0m));
    }

    [Fact]
    public void ForAdjustment_admite_ambos_signos()
    {
        Assert.Equal(500m, BalanceEntry.ForAdjustment(BarberId, 500m).Amount);
        Assert.Equal(-500m, BalanceEntry.ForAdjustment(BarberId, -500m).Amount);
    }

    [Fact]
    public void Un_movimiento_debe_pertenecer_a_un_barbero()
    {
        Assert.Throws<BusinessRuleException>(() => BalanceEntry.ForAdjustment(Guid.Empty, 100m));
    }
}
