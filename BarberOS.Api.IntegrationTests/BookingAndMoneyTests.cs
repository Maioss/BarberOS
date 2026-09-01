using System.Net;
using System.Net.Http.Json;

namespace BarberOS.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class BookingAndMoneyTests(ApiFixture api)
{
    private sealed record Paged<T>(List<T> Items, int TotalCount, int Page, int PageSize);
    private sealed record Shop(Guid Id, string Name, bool IsMain, Guid? ParentId);
    private sealed record Barber(Guid Id, string FullName, Guid BarbershopId);
    private sealed record Service(Guid Id, Guid BarbershopId, string Name, decimal Price, int DurationMinutes);
    private sealed record Appointment(Guid Id, Guid ClientId, string ClientName, Guid BarberId, decimal TotalPrice, string Status);
    private sealed record Payment(Guid Id, Guid AppointmentId, decimal Amount, string Status);
    private sealed record BalanceInfo(Guid BarberId, decimal Balance);
    private sealed record Availability(Guid BarberId, DateOnly Date, bool IsWorkingDay, List<Slot> Slots);
    private sealed record Slot(TimeOnly Start, TimeOnly End);

    private async Task<(Barber Barber, Service Service)> BookableBarber()
    {
        var anonymous = api.Anonymous();
        var branches = await anonymous.GetData<Paged<Shop>>("/api/barbershops?isMain=false&pageSize=100");

        foreach (var branch in branches.Items)
        {
            var barbers = await anonymous.GetData<List<Barber>>($"/api/barbershops/{branch.Id}/barbers");
            var services = await anonymous.GetData<List<Service>>($"/api/barbershops/{branch.Id}/services");

            if (barbers.Count > 0 && services.Count > 0)
                return (barbers[0], services[0]);
        }

        throw new InvalidOperationException("No hay barberos con servicios sembrados.");
    }

    private async Task<(Barber Barber, Service Service)> BookableBarberUnder(Guid mainId)
    {
        var anonymous = api.Anonymous();
        var shops = await anonymous.GetData<Paged<Shop>>("/api/barbershops?pageSize=100");

        foreach (var branch in shops.Items.Where(s => s.ParentId == mainId))
        {
            var barbers = await anonymous.GetData<List<Barber>>($"/api/barbershops/{branch.Id}/barbers");
            var services = await anonymous.GetData<List<Service>>($"/api/barbershops/{branch.Id}/services");

            if (barbers.Count > 0 && services.Count > 0)
                return (barbers[0], services[0]);
        }

        throw new InvalidOperationException($"La sede {mainId} no tiene sucursales con barberos y servicios.");
    }

    private static DateTime ShopLocalNow() => TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Bogota"));

    private static DateOnly ShopToday() => DateOnly.FromDateTime(ShopLocalNow());

    private static TimeOnly ShopNow() => TimeOnly.FromDateTime(ShopLocalNow());

    private static DateOnly NextMonday(int weeksAhead)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7 * weeksAhead);
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);
        return date;
    }

    private static object Booking(Guid barberId, DateOnly date, TimeOnly start, Guid serviceId, string? notes = null, Guid? clientId = null) =>
        new { barberId, date, startTime = start, serviceIds = new[] { serviceId }, notes, clientId };


    [Fact]
    public async Task Un_cliente_no_puede_reservar_a_nombre_de_otro()
    {
        var client = api.AsClient();
        var victimClient = api.AsOtherClient();
        var victim = await victimClient.GetData<ApiFixture.UserData>("/api/users/me");
        var (barber, service) = await BookableBarber();

        var response = await client.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, NextMonday(3), new TimeOnly(9, 0), service.Id, clientId: victim.Id));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Un_admin_si_puede_reservar_a_nombre_de_un_cliente()
    {
        var admin = api.AsAdmin();
        var clientHttp = api.AsClient();
        var target = await clientHttp.GetData<ApiFixture.UserData>("/api/users/me");

        var mine = (await admin.GetData<ApiFixture.UserData>("/api/users/me")).BarbershopId!.Value;
        var (barber, service) = await BookableBarberUnder(mine);

        var response = await admin.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, NextMonday(4), new TimeOnly(11, 0), service.Id, clientId: target.Id));

        var created = await response.ReadData<Appointment>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(target.Id, created.ClientId);
    }


    [Fact]
    public async Task La_disponibilidad_de_hoy_no_ofrece_horarios_que_ya_pasaron()
    {
        var (barber, _) = await BookableBarber();
        var today = ShopToday();
        var now = ShopNow();

        var availability = await api.Anonymous()
            .GetData<Availability>($"/api/barbers/{barber.Id}/availability?date={today:yyyy-MM-dd}");

        Assert.All(availability.Slots, slot => Assert.True(slot.Start >= now,
            $"El slot {slot.Start} ya paso (son las {now})."));
    }

    [Fact]
    public async Task No_se_puede_reservar_un_horario_de_hoy_que_ya_paso()
    {
        var client = api.AsClient();
        var (barber, service) = await BookableBarber();
        var today = ShopToday();

        var response = await client.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, today, new TimeOnly(0, 30), service.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task No_se_puede_reservar_en_una_fecha_pasada()
    {
        var client = api.AsClient();
        var (barber, service) = await BookableBarber();

        var response = await client.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), new TimeOnly(10, 0), service.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task Dos_reservas_que_se_solapan_no_pueden_coexistir()
    {
        var first = api.AsClient();
        var second = api.AsOtherClient();
        var (barber, service) = await BookableBarber();
        var date = NextMonday(5);

        var ok = await first.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, date, new TimeOnly(9, 0), service.Id, "primera"));
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);

        var clash = await second.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, date, new TimeOnly(9, 0), service.Id, "segunda"));

        Assert.True(
            clash.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest,
            $"Se esperaba un rechazo y llego {clash.StatusCode}.");
    }

    [Fact]
    public async Task Reservas_simultaneas_al_mismo_hueco_dejan_solo_una()
    {
        var (barber, service) = await BookableBarber();
        var date = NextMonday(6);
        var start = new TimeOnly(14, 0);

        var clients = new[]
        {
            api.AsClient(),
            api.AsOtherClient(),
            api.AsClient(),
            api.AsOtherClient()
        };

        var responses = await Task.WhenAll(clients.Select((c, i) =>
            c.PostAsJsonAsync("/api/appointments", Booking(barber.Id, date, start, service.Id, $"carrera-{i}"))));

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        Assert.Equal(1, created);
    }


    [Fact]
    public async Task Completar_una_cita_acredita_su_total_al_saldo_del_barbero()
    {
        var admin = api.AsSuperAdmin();
        var barberHttp = api.AsBarber();
        var before = await barberHttp.GetData<BalanceInfo>("/api/barbers/me/balance");

        var appointment = await CompleteAppointmentAsSuperAdmin();

        var after = await barberHttp.GetData<BalanceInfo>("/api/barbers/me/balance");
        Assert.Equal(before.Balance + appointment.TotalPrice, after.Balance);
    }

    [Fact]
    public async Task El_pago_toma_el_monto_de_la_cita_y_no_lo_que_manda_el_cliente()
    {
        var admin = api.AsSuperAdmin();
        var appointment = await CompleteAppointmentAsSuperAdmin();

        var response = await admin.PostAsJsonAsync("/api/payments", new
        {
            appointmentId = appointment.Id,
            method = "Cash",
            amount = 999_999m,
            notes = "intento de inflar"
        });

        var payment = await response.ReadData<Payment>();
        Assert.Equal(appointment.TotalPrice, payment.Amount);
    }

    [Fact]
    public async Task Reembolsar_deja_el_saldo_como_estaba_antes_de_completar()
    {
        var admin = api.AsSuperAdmin();
        var barberHttp = api.AsBarber();
        var before = await barberHttp.GetData<BalanceInfo>("/api/barbers/me/balance");

        var appointment = await CompleteAppointmentAsSuperAdmin();
        var payment = await (await admin.PostAsJsonAsync("/api/payments",
            new { appointmentId = appointment.Id, method = "Cash", notes = (string?)null })).ReadData<Payment>();

        var refund = await admin.PatchAsync($"/api/payments/{payment.Id}/refund", null);
        refund.EnsureSuccessStatusCode();

        var after = await barberHttp.GetData<BalanceInfo>("/api/barbers/me/balance");
        Assert.Equal(before.Balance, after.Balance);
    }

    [Fact]
    public async Task Un_pago_no_se_puede_reembolsar_dos_veces()
    {
        var admin = api.AsSuperAdmin();
        var appointment = await CompleteAppointmentAsSuperAdmin();
        var payment = await (await admin.PostAsJsonAsync("/api/payments",
            new { appointmentId = appointment.Id, method = "Cash", notes = (string?)null })).ReadData<Payment>();

        (await admin.PatchAsync($"/api/payments/{payment.Id}/refund", null)).EnsureSuccessStatusCode();
        var second = await admin.PatchAsync($"/api/payments/{payment.Id}/refund", null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task<Appointment> CompleteAppointmentAsSuperAdmin()
    {
        var barberHttp = api.AsBarber();
        var me = await barberHttp.GetData<ApiFixture.UserData>("/api/users/me");
        var branchId = me.BarbershopId!.Value;

        var barbers = await api.Anonymous().GetData<List<Barber>>($"/api/barbershops/{branchId}/barbers");
        var services = await api.Anonymous().GetData<List<Service>>($"/api/barbershops/{branchId}/services");
        var barber = barbers.First(b => b.BarbershopId == branchId);

        var availability = await api.Anonymous()
            .GetData<Availability>($"/api/barbers/{barber.Id}/availability?date={ShopToday():yyyy-MM-dd}");

        Assert.True(availability.Slots.Count > 0,
            $"No quedan huecos hoy ({ShopToday():yyyy-MM-dd} {ShopNow():HH\\:mm}) para completar una cita.");

        var client = api.AsClient();
        var created = await (await client.PostAsJsonAsync("/api/appointments",
            Booking(barber.Id, ShopToday(), availability.Slots[0].Start, services[0].Id, $"cobro-{Guid.NewGuid():N}")))
            .ReadData<Appointment>();

        var superAdmin = api.AsSuperAdmin();
        (await superAdmin.PatchAsync($"/api/appointments/{created.Id}/complete", null)).EnsureSuccessStatusCode();
        return created;
    }


    [Fact]
    public async Task Filtrar_pagos_por_fecha_no_revienta()
    {
        var admin = api.AsAdmin();

        var response = await admin.GetAsync("/api/payments?dateFrom=2026-01-01&dateTo=2026-12-31");

        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("/api/appointments?pageSize=100000")]
    [InlineData("/api/payments?pageSize=100000")]
    public async Task La_paginacion_esta_topada(string url)
    {
        var admin = api.AsAdmin();

        var page = await admin.GetData<Paged<object>>(url);

        Assert.Equal(100, page.PageSize);
    }

    [Fact]
    public async Task Subir_una_foto_sin_adjuntar_archivo_es_un_error_del_cliente()
    {
        var client = api.AsClient();
        using var form = new MultipartFormDataContent
        {
            { new StringContent("no soy un archivo"), "otroCampo" }
        };

        var response = await client.PostAsync("/api/users/me/photo/upload", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Un_archivo_que_no_es_imagen_se_rechaza_aunque_diga_que_lo_es()
    {
        var client = api.AsClient();
        var content = new ByteArrayContent("<html>no soy una imagen</html>"u8.ToArray());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        using var form = new MultipartFormDataContent { { content, "file", "falsa.jpg" } };
        var response = await client.PostAsync("/api/users/me/photo/upload", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("JPEG", await response.ReadMessage());
    }

    [Fact]
    public async Task Una_imagen_real_se_guarda_como_ruta_relativa_y_se_sirve()
    {
        var client = api.AsClient();
        var png = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00, 0x00, 0x00,
            0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49,
            0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        };
        var content = new ByteArrayContent(png);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        using var form = new MultipartFormDataContent { { content, "file", "real.png" } };
        var updated = await (await client.PostAsync("/api/users/me/photo/upload", form))
            .ReadData<ApiFixture.UserData>();

        Assert.NotNull(updated.PhotoUrl);
        Assert.StartsWith("/photos/", updated.PhotoUrl);
        (await client.GetAsync(updated.PhotoUrl)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task No_se_puede_apuntar_la_foto_a_un_servidor_ajeno()
    {
        var client = api.AsClient();

        var response = await client.PutAsJsonAsync("/api/users/me/photo",
            new { photoUrl = "https://evil.com/track.gif" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
