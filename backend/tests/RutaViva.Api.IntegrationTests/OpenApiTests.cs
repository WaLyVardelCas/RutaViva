using System.Net;
using System.Text.Json;

namespace RutaViva.Api.IntegrationTests;

public class OpenApiTests
{
    [Fact]
    public async Task Swagger_documents_contracts_and_marks_support_operations()
    {
        await using ApiFactory factory = new ApiFactory();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json");
        string body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument document = JsonDocument.Parse(body);

        Assert.Contains("Operaciones de soporte para la demostración", document.RootElement.GetProperty("tags").GetRawText());
        JsonElement paths = document.RootElement.GetProperty("paths");
        AssertSupport(paths, "/api/telemetry/readings", "400", "404", "409");
        AssertSupport(paths, "/api/processes/daily-evaluation", "400");

        JsonElement quote = paths.GetProperty("/api/orders/{id}/quote").GetProperty("post");
        Assert.Equal("Órdenes", quote.GetProperty("tags")[0].GetString());
        Assert.True(quote.GetProperty("responses").TryGetProperty("409", out JsonElement conflict));
        Assert.Contains("ApiError", conflict.GetRawText());

        JsonElement schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("VehicleRequest", out _));
        Assert.True(schemas.TryGetProperty("VehicleDto", out _));
        Assert.True(schemas.TryGetProperty("RoutineDto", out _));
        Assert.True(schemas.TryGetProperty("OrderDto", out _));
        Assert.True(schemas.TryGetProperty("ApiError", out _));
        Assert.True(schemas.TryGetProperty("EvaluationResponse", out JsonElement evaluation));
        Assert.True(evaluation.GetProperty("properties").TryGetProperty("routinesCambiadas", out _));
        Assert.True(evaluation.GetProperty("properties").TryGetProperty("vehiclesFallidos", out _));

        JsonElement reading = schemas.GetProperty("ReadingResponse").GetProperty("properties").GetProperty("result");
        JsonElement resultSchema = reading.TryGetProperty("$ref", out JsonElement reference)
            ? schemas.GetProperty(reference.GetString()!.Split('/').Last())
            : reading;
        Assert.Contains("Accepted", resultSchema.GetRawText());
        Assert.Contains("Ignored", resultSchema.GetRawText());
    }

    private static void AssertSupport(JsonElement paths, string path, params string[] errors)
    {
        JsonElement operation = paths.GetProperty(path).GetProperty("post");
        Assert.Equal(OpenApi.SupportTag, operation.GetProperty("tags")[0].GetString());
        Assert.Contains("Demostración", operation.GetProperty("summary").GetString());
        JsonElement responses = operation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("200", out _));
        Assert.True(responses.TryGetProperty("500", out _));
        foreach (string error in errors)
            Assert.True(responses.TryGetProperty(error, out _), path + " " + error);
    }
}
