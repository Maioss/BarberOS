using BarberOS.Domain.Entities;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Tests;

public class ServiceTests
{
    private static Service Create(decimal price = 25000m, int minutes = 30) =>
        Service.Create(Guid.NewGuid(), " Corte ", "  desc  ", price, minutes);

    [Fact]
    public void Create_recorta_nombre_y_descripcion()
    {
        var service = Create();

        Assert.Equal("Corte", service.Name);
        Assert.Equal("desc", service.Description);
    }

    [Fact]
    public void Create_admite_precio_cero()
    {
        Assert.Equal(0m, Create(price: 0m).Price);
    }

    [Fact]
    public void Create_rechaza_precio_negativo()
    {
        Assert.Throws<BusinessRuleException>(() => Create(price: -1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_exige_duracion_positiva(int minutes)
    {
        Assert.Throws<BusinessRuleException>(() => Create(minutes: minutes));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(31)]
    public void Create_exige_duracion_multiplo_de_cinco(int minutes)
    {
        var ex = Assert.Throws<BusinessRuleException>(() => Create(minutes: minutes));
        Assert.Contains("múltiplo de 5", ex.Message);
    }

    [Fact]
    public void Update_revalida_las_mismas_reglas()
    {
        var service = Create();

        Assert.Throws<BusinessRuleException>(() => service.Update("Corte", null, -1m, 30));
        Assert.Throws<BusinessRuleException>(() => service.Update("Corte", null, 100m, 7));
        Assert.Equal(25000m, service.Price);
    }

    [Fact]
    public void Deactivate_apaga_el_servicio()
    {
        var service = Create();

        service.Deactivate();

        Assert.False(service.IsActive);
    }
}
