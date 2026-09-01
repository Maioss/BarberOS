using System.Net;
using System.Net.Http.Json;

namespace BarberOS.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AccessControlTests(ApiFixture api)
{
    private sealed record Paged<T>(List<T> Items, int TotalCount, int Page, int PageSize);
    private sealed record Shop(Guid Id, string Name, bool IsMain, Guid? ParentId);
    private sealed record Barber(Guid Id, string FullName, Guid BarbershopId);
    private sealed record Appointment(Guid Id, Guid ClientId, string ClientName, Guid BarbershopId);
    private sealed record Service(Guid Id, Guid BarbershopId, string Name);

    private static object NewShop(string name) =>
        new { name, address = "Calle 1", city = "Cali", phone = (string?)null, isMain = true, parentId = (Guid?)null };

    private async Task<Guid> AdminBarbershopId()
    {
        var client = await api.AsAdmin();
        var me = await client.GetData<ApiFixture.UserData>("/api/users/me");
        return me.BarbershopId!.Value;
    }

    private async Task<Shop> ForeignMainShopWithServices()
    {
        var mine = await AdminBarbershopId();
        var anonymous = api.Anonymous();
        var shops = await anonymous.GetData<Paged<Shop>>("/api/barbershops?isMain=true&pageSize=100");

        foreach (var shop in shops.Items.Where(s => s.Id != mine))
        {
            var services = await anonymous.GetData<List<Service>>($"/api/barbershops/{shop.Id}/services");
            if (services.Count > 0) return shop;
        }

        throw new InvalidOperationException("No hay otra barbería principal con servicios sembrados.");
    }


    [Fact]
    public async Task Un_cliente_no_puede_crear_barberias()
    {
        var client = await api.AsClient();

        var response = await client.PostAsJsonAsync("/api/barbershops", NewShop("Sede del cliente"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_tampoco_puede_crear_barberias()
    {
        var client = await api.AsAdmin();

        var response = await client.PostAsJsonAsync("/api/barbershops", NewShop("Sede del admin"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_cliente_no_puede_renombrar_una_barberia()
    {
        var client = await api.AsClient();
        var shop = await ForeignMainShopWithServices();

        var response = await client.PutAsJsonAsync($"/api/barbershops/{shop.Id}",
            new { name = "Secuestrada", address = "x", city = "y", phone = (string?)null });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_cliente_no_puede_desactivar_una_sucursal()
    {
        var client = await api.AsClient();
        var branches = await api.Anonymous().GetData<Paged<Shop>>("/api/barbershops?isMain=false&pageSize=20");

        var response = await client.DeleteAsync($"/api/barbershops/{branches.Items[0].Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_superadmin_si_puede_crear_barberias()
    {
        var client = await api.AsSuperAdmin();

        var response = await client.PostAsJsonAsync("/api/barbershops", NewShop($"Sede QA {Guid.NewGuid():N}"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }


    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Admin")]
    public async Task Un_admin_no_puede_crear_cuentas_de_administrador(string role)
    {
        var client = await api.AsAdmin();

        var response = await client.PostAsJsonAsync("/api/users", new
        {
            email = $"escalada-{Guid.NewGuid():N}@evil.com",
            password = "Passw0rd123",
            fullName = "Cuenta Escalada",
            phone = (string?)null,
            role,
            barbershopId = (Guid?)null
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_no_puede_editar_al_superadmin()
    {
        var admin = await api.AsAdmin();
        var superAdmin = await api.AsSuperAdmin();
        var target = await superAdmin.GetData<ApiFixture.UserData>("/api/users/me");

        var response = await admin.PutAsJsonAsync($"/api/users/{target.Id}",
            new { fullName = "Secuestrado", phone = (string?)null, role = "SuperAdmin", barbershopId = (Guid?)null });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_no_puede_desactivar_al_superadmin()
    {
        var admin = await api.AsAdmin();
        var superAdmin = await api.AsSuperAdmin();
        var target = await superAdmin.GetData<ApiFixture.UserData>("/api/users/me");

        var response = await admin.DeleteAsync($"/api/users/{target.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_si_puede_crear_un_barbero_de_su_sede()
    {
        var admin = await api.AsAdmin();
        var response = await admin.PostAsJsonAsync("/api/users", new
        {
            email = $"barbero-{Guid.NewGuid():N}@barberos.com",
            password = "Passw0rd123",
            fullName = "Barbero Legitimo",
            phone = (string?)null,
            role = "Barber",
            barbershopId = await AdminBarbershopId()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }


    [Fact]
    public async Task Un_cliente_no_puede_leer_el_usuario_de_otro()
    {
        var client = await api.AsClient();
        var superAdmin = await api.AsSuperAdmin();
        var target = await superAdmin.GetData<ApiFixture.UserData>("/api/users/me");

        var response = await client.GetAsync($"/api/users/{target.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_cliente_si_puede_leer_su_propio_perfil()
    {
        var client = await api.AsClient();

        var me = await client.GetData<ApiFixture.UserData>("/api/users/me");

        Assert.Equal("cliente.pitch@barberos.com", me.Email);
    }

    [Fact]
    public async Task El_listado_publico_de_barberos_no_expone_telefono_ni_id_de_usuario()
    {
        var shops = await api.Anonymous().GetData<Paged<Shop>>("/api/barbershops?isMain=true&pageSize=5");
        var shopId = shops.Items[0].Id;

        var response = await api.Anonymous().GetAsync($"/api/barbershops/{shopId}/barbers");
        var raw = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("\"phone\"", raw);
        Assert.DoesNotContain("\"userId\"", raw);
    }

    [Fact]
    public async Task El_listado_administrativo_de_barberos_si_trae_el_telefono()
    {
        var admin = await api.AsAdmin();

        var response = await admin.GetAsync($"/api/barbers?barbershopId={await AdminBarbershopId()}");
        var raw = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"phone\"", raw);
    }

    [Fact]
    public async Task Un_cliente_no_puede_usar_el_listado_administrativo_de_barberos()
    {
        var client = await api.AsClient();

        var response = await client.GetAsync($"/api/barbers?barbershopId={await AdminBarbershopId()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    [Fact]
    public async Task Un_admin_no_ve_las_metricas_de_otra_barberia()
    {
        var admin = await api.AsAdmin();
        var foreign = await ForeignMainShopWithServices();

        var response = await admin.GetAsync($"/api/metrics/barbershop/{foreign.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_si_ve_las_metricas_de_la_suya()
    {
        var admin = await api.AsAdmin();

        var response = await admin.GetAsync($"/api/metrics/barbershop/{await AdminBarbershopId()}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Un_admin_no_puede_crear_servicios_en_otra_barberia()
    {
        var admin = await api.AsAdmin();
        var foreign = await ForeignMainShopWithServices();

        var response = await admin.PostAsJsonAsync("/api/services", new
        {
            barbershopId = foreign.Id,
            name = "Servicio intruso",
            description = (string?)null,
            price = 1000m,
            durationMinutes = 10
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_no_puede_editar_un_servicio_de_otra_barberia()
    {
        var admin = await api.AsAdmin();
        var foreign = await ForeignMainShopWithServices();
        var services = await api.Anonymous().GetData<List<Service>>($"/api/barbershops/{foreign.Id}/services");

        var response = await admin.PutAsJsonAsync($"/api/services/{services[0].Id}",
            new { name = "Secuestrado", description = (string?)null, price = 1m, durationMinutes = 5 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_no_puede_listar_los_barberos_de_otra_barberia()
    {
        var admin = await api.AsAdmin();
        var foreign = await ForeignMainShopWithServices();

        var response = await admin.GetAsync($"/api/barbers?barbershopId={foreign.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_solo_ve_las_citas_de_su_barberia()
    {
        var admin = await api.AsAdmin();
        var superAdmin = await api.AsSuperAdmin();
        var mine = await AdminBarbershopId();

        var sitesOfMine = await api.Anonymous().GetData<Paged<Shop>>("/api/barbershops?pageSize=100");
        var allowed = sitesOfMine.Items
            .Where(s => s.Id == mine || s.ParentId == mine)
            .Select(s => s.Id)
            .ToHashSet();

        var visible = await admin.GetData<Paged<Appointment>>("/api/appointments?pageSize=100");
        var all = await superAdmin.GetData<Paged<Appointment>>("/api/appointments?pageSize=100");

        Assert.All(visible.Items, a => Assert.Contains(a.BarbershopId, allowed));
        Assert.True(all.TotalCount >= visible.TotalCount);
    }

    [Fact]
    public async Task Un_superadmin_no_tiene_restriccion_de_sede()
    {
        var superAdmin = await api.AsSuperAdmin();
        var foreign = await ForeignMainShopWithServices();

        var response = await superAdmin.GetAsync($"/api/metrics/barbershop/{foreign.Id}");

        response.EnsureSuccessStatusCode();
    }
}
