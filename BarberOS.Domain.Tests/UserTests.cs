using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Tests;

public class UserTests
{
    private static User Client() =>
        User.Create("Cliente@BarberOS.com ", "hash", " Carlos ", Role.Client);

    [Fact]
    public void Create_normaliza_el_correo_y_recorta_el_nombre()
    {
        var user = Client();

        Assert.Equal("cliente@barberos.com", user.Email);
        Assert.Equal("Carlos", user.FullName);
    }

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Barber)]
    public void Create_exige_barberia_para_admin_y_barbero(Role role)
    {
        var ex = Assert.Throws<BusinessRuleException>(() =>
            User.Create("a@b.com", "hash", "Nombre", role));

        Assert.Contains("barbería", ex.Message);
    }

    [Theory]
    [InlineData(Role.SuperAdmin)]
    [InlineData(Role.Client)]
    public void Create_prohibe_barberia_para_superadmin_y_cliente(Role role)
    {
        Assert.Throws<BusinessRuleException>(() =>
            User.Create("a@b.com", "hash", "Nombre", role, barbershopId: Guid.NewGuid()));
    }

    [Fact]
    public void ChangeRole_a_barbero_exige_barberia()
    {
        var user = Client();

        Assert.Throws<BusinessRuleException>(() => user.ChangeRole(Role.Barber, null));
        Assert.Equal(Role.Client, user.Role);
    }

    [Fact]
    public void ChangeRole_de_barbero_a_cliente_suelta_la_barberia()
    {
        var user = User.Create("b@b.com", "hash", "Barbero", Role.Barber, barbershopId: Guid.NewGuid());

        user.ChangeRole(Role.Client, null);

        Assert.Equal(Role.Client, user.Role);
        Assert.Null(user.BarbershopId);
    }

    [Fact]
    public void Create_no_arranca_con_foto()
    {
        Assert.Null(Client().PhotoUrl);
    }

    [Fact]
    public void UpdatePhoto_admite_null_para_quitarla()
    {
        var user = Client();
        user.UpdatePhoto("/photos/x.jpg");

        user.UpdatePhoto(null);

        Assert.Null(user.PhotoUrl);
    }
}
