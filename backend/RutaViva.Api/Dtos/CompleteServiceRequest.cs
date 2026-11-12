using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Datos reales con los que se cierra el servicio.</summary>
public sealed record CompleteServiceRequest(
    [property: Description("Kilometraje real del servicio. Obligatorio. No puede ser anterior a la referencia del ciclo.")] int? Mileage,
    [property: Description("Fecha real del servicio, YYYY-MM-DD. Obligatoria. No puede ser anterior a la referencia del ciclo.")] DateOnly? Date);
