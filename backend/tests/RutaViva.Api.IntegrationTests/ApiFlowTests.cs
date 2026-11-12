using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RutaViva.Domain;

namespace RutaViva.Api.IntegrationTests;

public class ApiFlowTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Happy_path_over_http()
    {
        await using ApiFactory factory = new ApiFactory();
        var client = factory.CreateClient();

        var vehicle = await Post<VehicleResponse>(client, "/api/vehicles", new { plate = "FLU001", mileage = 20_000 });
        var routine = await Post<RoutineResponse>(client, $"/api/vehicles/{vehicle.Id}/routines", new { name = "Aceite", kilometers = 10_000, date = "2026-01-01" });
        Assert.Equal("UpToDate", routine.Status);

        var dueSoonReading = await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "f1", plate = "FLU001", mileage = 29_000, readingDate = "2026-06-01" });
        Assert.Equal("Accepted", dueSoonReading.Result);
        var routines = await Get<List<RoutineResponse>>(client, $"/api/vehicles/{vehicle.Id}/routines");
        Assert.Equal("DueSoon", routines.Single(item => item.Id == routine.Id).Status);
        Assert.Equal("DueSoon", routines.Single(item => item.Name == DomainConstants.OilChange).Status);

        await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "f2", plate = "FLU001", mileage = 30_000, readingDate = "2026-07-01" });
        var order = Assert.Single(await Get<List<OrderResponse>>(client, $"/api/orders?vehicleId={vehicle.Id}"), item => item.RoutineId == routine.Id);
        Assert.Equal("Created", order.Status);

        var quoted = await Post<OrderResponse>(client, $"/api/orders/{order.Id}/quote", new { amount = 900_000 });
        Assert.Equal("Approved", quoted.Status);
        Assert.True(quoted.AutomaticApproval);

        var inProgress = await Post<OrderResponse>(client, $"/api/orders/{order.Id}/start", new { });
        Assert.Equal("InProgress", inProgress.Status);
        var completedOrder = await Post<OrderResponse>(client, $"/api/orders/{order.Id}/completion", new { mileage = 30_000, date = "2026-07-05" });
        Assert.Equal("Completed", completedOrder.Status);

        routines = await Get<List<RoutineResponse>>(client, $"/api/vehicles/{vehicle.Id}/routines");
        Assert.Equal("UpToDate", routines.Single(item => item.Id == routine.Id).Status);
        Assert.Equal("Overdue", routines.Single(item => item.Name == DomainConstants.OilChange).Status);
        Assert.Equal(30_000, (await Get<VehicleResponse>(client, $"/api/vehicles/{vehicle.Id}")).CurrentMileage);
    }

    [Fact]
    public async Task Rejecting_the_quote_cancels_the_order()
    {
        await using ApiFactory factory = new ApiFactory();
        var client = factory.CreateClient();
        var vehicle = await Post<VehicleResponse>(client, "/api/vehicles", new { plate = "REC001", mileage = 20_000 });
        var routine = await Post<RoutineResponse>(client, $"/api/vehicles/{vehicle.Id}/routines", new { name = "Frenos", kilometers = 10_000, date = "2026-01-01" });
        await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "r1", plate = "REC001", mileage = 30_000, readingDate = "2026-07-01" });
        var order = Assert.Single(await Get<List<OrderResponse>>(client, $"/api/orders?status=Created&vehicleId={vehicle.Id}"), item => item.RoutineId == routine.Id);

        var quoted = await Post<OrderResponse>(client, $"/api/orders/{order.Id}/quote", new { amount = 2_000_000 });
        Assert.Equal("Quoted", quoted.Status);
        var rejectedOrder = await Post<OrderResponse>(client, $"/api/orders/{order.Id}/rejection", new { });
        Assert.Equal("Canceled", rejectedOrder.Status);

        var stored = await Get<List<RoutineResponse>>(client, $"/api/vehicles/{vehicle.Id}/routines");
        Assert.Equal("Overdue", stored.Single(item => item.Id == routine.Id).Status);
    }

    [Fact]
    public async Task An_ignored_reading_returns_200_and_a_replay_does_not_change_mileage()
    {
        await using ApiFactory factory = new ApiFactory();
        var client = factory.CreateClient();
        await Post<VehicleResponse>(client, "/api/vehicles", new { plate = "IGN001", mileage = 25_000 });

        var ignored = await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "i1", plate = "IGN001", mileage = 24_000, readingDate = "2026-03-09" });
        Assert.Equal("Ignored", ignored.Result);

        await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "i2", plate = "IGN001", mileage = 26_000, readingDate = "2026-03-10" });
        var replayed = await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "i2", plate = "IGN001", mileage = 40_000, readingDate = "2026-03-11" });
        Assert.Equal("Accepted", replayed.Result);

        var vehicles = await Get<List<VehicleResponse>>(client, "/api/vehicles");
        Assert.Equal(26_000, Assert.Single(vehicles, v => v.Plate == "IGN001").CurrentMileage);
    }

    [Fact]
    public async Task Missing_date_and_invalid_transition_fail()
    {
        await using ApiFactory factory = new ApiFactory();
        var client = factory.CreateClient();

        var missingDate = await client.PostAsJsonAsync("/api/processes/daily-evaluation", new { });
        Assert.Equal(HttpStatusCode.BadRequest, missingDate.StatusCode);
        var malformed = await client.PostAsync("/api/processes/daily-evaluation", new StringContent("{\"date\":\"ayer\"}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);

        var vehicle = await Post<VehicleResponse>(client, "/api/vehicles", new { plate = "ERR001", mileage = 0 });
        await Post<RoutineResponse>(client, $"/api/vehicles/{vehicle.Id}/routines", new { name = "Aceite", kilometers = 1_000, date = "2026-01-01" });
        await Post<ReadingResponse>(client, "/api/telemetry/readings", new { eventId = "e1", plate = "ERR001", mileage = 1_000, readingDate = "2026-02-01" });
        var order = Assert.Single(await Get<List<OrderResponse>>(client, $"/api/orders?vehicleId={vehicle.Id}"));
        var started = await client.PostAsync($"/api/orders/{order.Id}/start", JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.Conflict, started.StatusCode);
    }

    [Fact]
    public async Task There_is_no_operation_to_edit_the_interval()
    {
        await using ApiFactory factory = new ApiFactory();
        var client = factory.CreateClient();
        var vehicle = await Post<VehicleResponse>(client, "/api/vehicles", new { plate = "EDI001", mileage = 0 });
        var routine = await Post<RoutineResponse>(client, $"/api/vehicles/{vehicle.Id}/routines", new { name = "Aceite", kilometers = 1_000 });
        var updateAttempt = await client.PatchAsync($"/api/vehicles/{vehicle.Id}/routines/{routine.Id}", JsonContent.Create(new { kilometers = 2_000 }));
        Assert.Equal(HttpStatusCode.NotFound, updateAttempt.StatusCode);
    }

    private static async Task<T> Post<T>(HttpClient client, string path, object body)
    {
        var response = await client.PostAsJsonAsync(path, body);
        var bodyText = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, bodyText);
        return JsonSerializer.Deserialize<T>(bodyText, Json)!;
    }

    private static async Task<T> Get<T>(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        var bodyText = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, bodyText);
        return JsonSerializer.Deserialize<T>(bodyText, Json)!;
    }

    private sealed record VehicleResponse(Guid Id, string Plate, int CurrentMileage);
    private sealed record RoutineResponse(Guid Id, string Name, string Status);
    private sealed record ReadingResponse(string Result);
    private sealed record OrderResponse(Guid Id, Guid RoutineId, string Status, bool? AutomaticApproval);
}
