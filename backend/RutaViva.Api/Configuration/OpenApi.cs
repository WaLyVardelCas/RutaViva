using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using RutaViva.Application;
using RutaViva.Domain;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RutaViva.Api;

public static class OpenApi
{
    public const string SupportTag = "Soporte";

    public static void AddRutaVivaOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RutaViva",
                Version = "v1",
                Description = "Mantenimiento preventivo. Vehículos, rutinas y órdenes son el contrato del portal. "
                    + "El grupo Soporte reúne la simulación de telemetría y la evaluación diaria: son operaciones de demostración y el portal no las consume."
            });
            options.SupportNonNullableReferenceTypes();
            options.SchemaFilter<StringEnumSchemaFilter>();
            options.OperationFilter<ErrorResponsesFilter>();
            options.DocumentFilter<TagDescriptionsFilter>();

            string xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xml))
                options.IncludeXmlComments(xml);
        });
    }

    public static void UseRutaVivaOpenApi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "RutaViva v1");
            options.DocumentTitle = "RutaViva";
        });
    }
}

public sealed class StringEnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum || schema is not OpenApiSchema target)
            return;

        target.Type = JsonSchemaType.String;
        target.Format = null;
        target.Enum = Enum.GetNames(context.Type)
            .Select(name => (JsonNode)JsonValue.Create(name)!)
            .ToList();
        target.Description = context.Type switch
        {
            _ when context.Type == typeof(ReadingResult) =>
                "Accepted si el kilometraje se aplicó. Ignored si no supera el actual o si el eventId ya fue procesado.",
            _ when context.Type == typeof(OrderStatus) =>
                "Created, Quoted, Approved, InProgress, Completed o Canceled.",
            _ when context.Type == typeof(RoutineStatus) =>
                "UpToDate, DueSoon u Overdue. El estado no retrocede dentro del ciclo.",
            _ => target.Description
        };
    }
}

public sealed class ErrorResponsesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        OpenApiResponses responses = operation.Responses ?? new OpenApiResponses();
        operation.Responses = responses;
        Describe(responses, "400", "Datos inválidos. Falta un campo obligatorio o el JSON no se puede leer.");
        Describe(responses, "404", "No encontrado.");
        Describe(responses, "409", "Regla de negocio: transición no permitida, dato rechazado o conflicto de concurrencia.");

        if (responses.ContainsKey("500"))
            return;

        responses.Add("500", new OpenApiResponse
        {
            Description = "Error interno. El detalle no expone la excepción.",
            Content = new Dictionary<string, IOpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(ApiError), context.SchemaRepository)
                }
            }
        });
    }

    private static void Describe(OpenApiResponses responses, string status, string description)
    {
        if (!responses.TryGetValue(status, out IOpenApiResponse? response) || response is null)
            return;
        if (string.IsNullOrWhiteSpace(response.Description))
            response.Description = description;
    }
}

public sealed class TagDescriptionsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        ISet<OpenApiTag> tags = document.Tags ?? new HashSet<OpenApiTag>();
        document.Tags = tags;
        Describe(tags, "Vehículos", "Registro y consulta de la flota.");
        Describe(tags, "Rutinas", "Configuración y consulta de las rutinas de un vehículo. El intervalo no se edita: se crea otra rutina.");
        Describe(tags, "Órdenes", "Cotización, decisión del cliente y ejecución del servicio.");
        Describe(tags, OpenApi.SupportTag,
            "Operaciones de soporte para la demostración. Simulan una lectura de telemetría y disparan la evaluación diaria con una fecha indicada. El portal no las consume; se prueban desde Swagger o con RutaViva.http.");
    }

    private static void Describe(ISet<OpenApiTag> tags, string name, string description)
    {
        OpenApiTag? tag = tags.FirstOrDefault(item => item.Name == name);
        if (tag is null)
        {
            tags.Add(new OpenApiTag { Name = name, Description = description });
            return;
        }

        tag.Description = description;
    }
}
